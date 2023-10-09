using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;

namespace Chief.PageTransitions;

public class DrillTransition : IPageTransition
{
    public bool Backward { get; set; }

    public TimeSpan Duration { get; set; } = TimeSpan.FromMilliseconds(300);

    public async Task Start(Visual? from, Visual? to, bool forward, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        var animations = new List<Task>();

        if (from != null)
        {
            animations.Add(Fade(false).RunAsync(from, cancellationToken));
            if (Backward)
            {
                animations.Add(Drill(ScaleTransform.ScaleXProperty, false).RunAsync(from, cancellationToken));
                animations.Add(Drill(ScaleTransform.ScaleYProperty, false).RunAsync(from, cancellationToken));
            }
        }

        if (to != null)
        {
            to.IsVisible = true;
            animations.Add(Fade(true).RunAsync(to, cancellationToken));
            if (!Backward)
            {
                animations.Add(Drill(ScaleTransform.ScaleXProperty, true).RunAsync(to, cancellationToken));
                animations.Add(Drill(ScaleTransform.ScaleYProperty, true).RunAsync(to, cancellationToken));
            }
        }

        await Task.WhenAll(animations);

        if (from != null && !cancellationToken.IsCancellationRequested)
        {
            from.IsVisible = false;
        }
    }

    private Animation Fade(bool isOut) => new()
    {
        FillMode = FillMode.Forward,
        Duration = Duration,
        Easing = new SineEaseIn(),
        Children =
        {
            new KeyFrame()
            {
                Setters =
                {
                    new Setter()
                    {
                        Property = Visual.OpacityProperty,
                        Value = isOut ? 0.0d : 1.0d
                    }
                },
                Cue = new Cue(0d)
            },
            new KeyFrame()
            {
                Setters =
                {
                    new Setter()
                    {
                        Property = Visual.OpacityProperty,
                        Value = isOut ? 1.0d : 0.0d
                    }
                },
                Cue = new Cue(1.0d)
            }
        }
    };

    private Animation Drill(AvaloniaProperty prop, bool isOut) => new()
    {
        FillMode = FillMode.Forward,
        Easing = isOut ? new SineEaseOut() : new SineEaseIn(),
        Duration = Duration,
        Children =
        {
            new KeyFrame
            {
                Setters =
                {
                    new Setter
                    {
                        Property = prop,
                        Value = isOut ? 0.75d : 1.0d
                    }
                },
                Cue = new Cue(0.0d)
            },
            new KeyFrame
            {
                Setters =
                {
                    new Setter
                    {
                        Property = prop,
                        Value = isOut ? 1.0d : 0.75d
                    }
                },
                Cue = new Cue(1.0d)
            }
        }
    };
}