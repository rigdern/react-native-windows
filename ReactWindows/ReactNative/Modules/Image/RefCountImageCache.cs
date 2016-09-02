﻿using ReactNative.Bridge;
using ReactNative.Views.Image;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace ReactNative.Modules.Image
{
    class RefCountImageCache : IImageCache
    {
        private readonly IDictionary<string, ImageReference> _cache =
            new Dictionary<string, ImageReference>();

        private readonly IUriLoader _uriLoader;
         
        public RefCountImageCache(IUriLoader uriLoader)
        {
            _uriLoader = uriLoader;
        }

        public IImageReference Get(string uri)
        {
            return Get(new ReactImageRequest
            {
                Source = uri
            });
        }

        public IImageReference Get(ReactImageRequest imageRequest)
        {
            var uri = imageRequest.Source;
            var headers = imageRequest.Headers;
            var reference = default(ImageReference);
            lock (_cache)
            {
                if (_cache.TryGetValue(uri, out reference))
                {
                    reference.Increment();
                }
            }

            if (reference == null)
            {
                lock (_cache)
                {
                    if (!_cache.TryGetValue(uri, out reference))
                    {
                        reference = new ImageReference(imageRequest, this);
                        _cache.Add(uri, reference);
                    }
                    else
                    {
                        reference.Increment();
                    }
                }
            }

            return reference;
        }

        class ImageReference : IImageReference
        {
            private readonly RefCountImageCache _parent;
            private readonly ReplaySubject<Unit> _subject;

            private IDisposable _subscription;
            private int _refCount = 1;

            public ImageReference(ReactImageRequest imageRequest, RefCountImageCache parent)
            {
                Uri = imageRequest.Source;
                Headers = imageRequest.Headers;
                _parent = parent;
                _subject = new ReplaySubject<Unit>(1);
                InitializeImage();
            }

            public string Uri
            {
                get;
            }

            public IDictionary<string, string> Headers
            {
                get;
            }

            public BitmapImage Image
            {
                get;
                private set;
            }

            public IObservable<Unit> LoadedObservable
            {
                get
                {
                    return _subject;
                }
            }

            public void Increment()
            {
                Interlocked.Increment(ref _refCount);
            }

            public void Dispose()
            {
                lock (_parent._cache)
                {
                    if (Interlocked.Decrement(ref _refCount) == 0)
                    {
                        _subscription.Dispose();
                        _subject.Dispose();
                        _parent._cache.Remove(Uri);
                    }
                }
            }

            private void DoSubscribe(BitmapImage image)
            {
                var openedObservable = Observable.FromEventPattern<RoutedEventHandler, RoutedEventArgs>(
                    h => image.ImageOpened += h,
                    h => image.ImageOpened -= h)
                    .Select(_ => default(Unit));

                var failedObservable = Observable.FromEventPattern<ExceptionRoutedEventHandler, ExceptionRoutedEventArgs>(
                    h => image.ImageFailed += h,
                    h => image.ImageFailed -= h)
                    .Select<EventPattern<ExceptionRoutedEventArgs>, Unit>(pattern =>
                    {
                        throw new Exception(pattern.EventArgs.ErrorMessage);
                    });

                _subscription = openedObservable
                    .Merge(failedObservable)
                    .Subscribe(_subject);
            }

            private async void InitializeImage()
            {
                var stream = await _parent._uriLoader.OpenReadAsync(Uri, Headers);
                DispatcherHelpers.RunOnDispatcher(async () =>
                {
                    Image = new BitmapImage();
                    DoSubscribe(Image);
                    try
                    {
                        await Image.SetSourceAsync(stream);
                    }
                    finally
                    {
                        stream.Dispose();
                    }
                });
            }
        }
    }
}
