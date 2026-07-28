using Godot;
using System.Collections.Generic;

namespace RedAlert2ModCode.Common.Utils;

public sealed class ScrollDragState
{
    public Vector2 DragStart;
    public int ScrollStartH;
    public int ScrollStartV;
    public bool IsDragging;
    public ScrollContainer Container;
    public bool Horizontal;
    public bool Vertical;
}

public static class ScrollDragHelper
{
    private const float DragThreshold = 5f;

    public static ScrollDragState EnableDragScroll(ScrollContainer container, bool horizontal = true, bool vertical = false)
    {
        var state = new ScrollDragState
        {
            Container = container,
            Horizontal = horizontal,
            Vertical = vertical
        };

        container.GuiInput += (InputEvent @event) => HandleInput(state, @event);

        AttachToAllChildren(container, state);

        return state;
    }

    public static void AttachDragSource(Control source, ScrollDragState state)
    {
        source.GuiInput += (InputEvent @event) => HandleInput(state, @event);
    }

    private static void AttachToAllChildren(Node parent, ScrollDragState state)
    {
        foreach (var child in parent.GetChildren())
        {
            if (child is Control ctrl)
            {
                ctrl.GuiInput += (InputEvent @event) => HandleInput(state, @event);
            }
            AttachToAllChildren(child, state);
        }
    }

    private static void HandleInput(ScrollDragState state, InputEvent @event)
    {
        if (@event is InputEventMouseButton mb)
        {
            if (mb.ButtonIndex == MouseButton.Left)
            {
                if (mb.Pressed)
                {
                    state.DragStart = mb.Position;
                    state.ScrollStartH = state.Container.ScrollHorizontal;
                    state.ScrollStartV = state.Container.ScrollVertical;
                    state.IsDragging = false;
                }
                else
                {
                    state.IsDragging = false;
                }
            }
        }
        else if (@event is InputEventMouseMotion mm)
        {
            if ((mm.ButtonMask & MouseButtonMask.Left) != 0)
            {
                Vector2 delta = mm.Position - state.DragStart;

                if (!state.IsDragging && delta.Length() > DragThreshold)
                {
                    state.IsDragging = true;
                }

                if (state.IsDragging)
                {
                    if (state.Horizontal)
                    {
                        state.Container.ScrollHorizontal = state.ScrollStartH - (int)delta.X;
                    }
                    if (state.Vertical)
                    {
                        state.Container.ScrollVertical = state.ScrollStartV - (int)delta.Y;
                    }
                }
            }
        }
    }
}
