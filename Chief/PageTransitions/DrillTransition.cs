using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Chief.Utils;

namespace Chief.PageTransitions;

public class DrillTransition : IPageTransition
{
    public bool Backward { get; init; }

    public async Task Start(Visual? from, Visual? to, bool forward, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        var animations = new List<Task>();

        if (from != null)
        {
            foreach (var child in from.GetVisualDescendants().OfType<Control>())
            {
                child.IsEnabled = false;
            }

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
        Duration = TimeSpan.FromMilliseconds(Cache.Config.Instance!.AnimationSpeed),
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
        Duration = TimeSpan.FromMilliseconds(Cache.Config.Instance!.AnimationSpeed),
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