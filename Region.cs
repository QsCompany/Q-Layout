using System;
using System.Collections;
using System.Drawing;
using static System.Math;
using RP = Qs.Technology.Area.RectangleProperty;
using RPC = Qs.Technology.Area.RectangePropertCallBack;
namespace Qs.Technology.Area
{
    public enum Alignment
    {
        Left = 1, Right = 2, HCenter = Left | Right,
        Top = 4, Bottom = 8, VCenter = Top | Bottom,
        Center = HCenter | VCenter
    }
    public struct SizeF
    {
        public float Width { get; }
        public float Height { get; }
        public SizeF(float w,float h)
        {
            Width = w;
            Height = h;
        }
    }
    public struct PointF
    {
        public float Width { get; }
        public float Height { get; }
        public PointF(float w, float h)
        {
            Width = w;
            Height = h;
        }
    }
    
    public class RectangePropertCallBack
    {
        private float _x, _y, _h, _w;
        public RP changes
        {
            get; private set;
        }
        public float X
        {
            get { return _x; }
            set
            {
                if (_x == value) return;
                changes |= RP.X;
                _x = value;
            }
        }
        public float Y
        {
            get { return _y; }
            set
            {
                if (_y == value) return;
                changes |= RP.Y;
                _y = value;
            }
        }
        public float Width
        {
            get { return _w; }
            set
            {
                if (_w == value) return;
                changes |= RP.Width;
                _w = value;
            }
        }
        public float Height
        {
            get { return _h; }
            set
            {
                if (_h == value) return;
                changes |= RP.Height;
                _h = value;
            }
        }

        public bool IsInitializing => version != null;

        internal void Set(float x, float y, float w, float h)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
        }

        internal void SetLocation(float x, float y)
        {
            X = x; Y = y;
        }
        private object version = null;
        public object GetControl()
        {
            if (version != null) return null;
            changes = 0;
            return version = new object();
        }
        public bool IsControledBy(object version) 
            =>  version == this.version;
        public void CancelControl(object version) { if (version != this.version)
                throw new Exception("Error Using CancelControl"); this.version = null; }

    }
    public enum RectangleProperty
    {
        None = 0,
        X = 1, Y = 2, Width = 4, Height = 8,
        Rectangle = 15,
        Location = 3,
        Size = 12
    }
    public delegate void Validator(Region sender, RPC p);
    public delegate void PropertyChanged(Region sender, RPC p, RP property);
    public delegate SizeF ParametrizedRendredSize(Region sender);
    public delegate SizeF RendredSize();
    public delegate void WidthChangedHandler(Region r);
    public delegate void HeightChangedHandler(Region r);


    /// <summary>
    /// Infinity Manager
    /// </summary>
    public partial class Region
    {
        //Infinity Manager

        private void IamNotInfinyWidth()
        {
            if (Parent != this)
                Parent.WidthChanged -= InfinityControlParentWidth;
        }
        private void IamInfinyWidth(Region newParent)
        {
            if (_parent != null)
                _parent.WidthChanged -= InfinityControlParentWidth;
            if (newParent != null || newParent != this)
                newParent.WidthChanged += InfinityControlParentWidth;
        }
        private void IamInfinyHeight(Region newParent)
        {
            if (_parent != null)
                _parent.HeightChanged -= InfinityControlParentHeight;
            if (newParent != this)
                newParent.HeightChanged += InfinityControlParentHeight;
        }
        private void IamNotInfinyHeight()
        {
            if (_parent != null)
                _parent.HeightChanged -= InfinityControlParentHeight;
        }

        private void InfinityControlParentWidth(Region r)
        {
            var t = values.GetControl();
            values.Width = r.ActualWidth - ActualLeft;
            OnPropertyChanged(t);
        }
        private void InfinityControlParentHeight(Region parent)
        {
            var t = values.GetControl();
            values.Width = parent.ActualWidth - ActualTop;
            OnPropertyChanged(t);
        }


    }


    /// <summary>
    /// Auto Resize Manager 
    /// </summary>
    public partial class Region
    {
        // Auto Resize Manager

        private void IamAutoWidth()
        {
            foreach (var c in children)
                ((Region)c).WidthChanged += NaNControlChildWidthChanged;
        }
        private void IamNotAutoWidth()
        {
            foreach (var c in children)
                ((Region)c).WidthChanged -= NaNControlChildWidthChanged;
        }
        private void IamAutoHeight()
        {
            foreach (var c in children)
                ((Region)c).HeightChanged += NaNControlChildHeightChanged;
        }
        private void IamNotAutoHeight()
        {
            foreach (var c in children)
                ((Region)c).HeightChanged -= NaNControlChildHeightChanged;
        }
        private void NaNControlChildWidthChanged(Region r)
        {
            var t = values.GetControl();
            var s = GetInnerWidth();
            values.Width = s;
            OnPropertyChanged(t);
        }
        private void NaNControlChildHeightChanged(Region r)
        {
            var t = values.GetControl();
            var s = GetInnerHeight();
            values.Height = s;
            OnPropertyChanged(t);
        }

    }


    /// <summary>
    /// Relationship Child<-->Parent
    /// </summary>
    public partial class Region
    {
        public void RemoveChild(Region child)
        {
            if (child.Parent != this)
                throw new Exception("i dont have this child");
            if (child.Parent == child) return;
            if (child == this || !children.Contains(children))
                throw new InvalidOperationException();
            children.Remove(child);
            PropertyChanged -= child.OnParentRegionChanged;
            child._parent = null;
            if (float.IsNaN(_width))
                child.WidthChanged -= NaNControlChildWidthChanged;
            if (float.IsNaN(_height))
                child.HeightChanged -= NaNControlChildHeightChanged;
            if (float.IsInfinity(child.Width))
                child.IamNotInfinyWidth();
            if (float.IsInfinity(child.Height))
                child.IamNotInfinyHeight();
        }
        public void AddChild(Region child)
        {
            if (child.Parent != null && child.Parent != child)
                if (child.Parent == this) return;
                else
                    throw new Exception("This region is owned by other region remove it first");
            if (child == this || children.Contains(children))
                throw new InvalidOperationException();
            children.Add(child);
            PropertyChanged += child.OnParentRegionChanged;
            child._parent = this;
            if (float.IsNaN(_width))
                child.WidthChanged += NaNControlChildWidthChanged;
            if (float.IsNaN(_height))
                child.HeightChanged += NaNControlChildHeightChanged;
            if (float.IsInfinity(child.Width))
            {
                child.IamInfinyWidth(this);
                child.InfinityControlParentWidth(this);
            }
            if (float.IsInfinity(child.Height))
            {
                child.IamInfinyHeight(this);
                child.InfinityControlParentHeight(this);
            }
            Validate();
        }

        public void SetParent(Region parentRegion) 
            => parentRegion.AddChild(this);
        public void RemoveParent(Region parent) 
            => parent.RemoveChild(this);
        private void OnParentRegionChanged(Region sender, RPC p, RP property)
        {
            if (IsInitializing) return;
            Validate();
        }


    }
    public partial class Region
    {
        #region Fields
        private Region Parent
        {
            get { return _parent ?? this; }
        }
        private Region _parent;
        private Alignment _aligment;
        public event PropertyChanged PropertyChanged;
        private float _width, _height, _left, _top;
        public readonly RPC values = new RPC();

        #endregion

        #region Region Parametres
        public float Left
        {
            get { return _left; }
            set
            {
                if (_left == value) return;
                _left = value;
                Validate();
            }
        }
        public float Top
        {
            get { return _top; }
            set
            {
                if (_top == value) return;
                _top = value;
                Validate();
            }
        }

        public float Width
        {
            get { return _width; }
            set
            {
                if (value == _width) return;
                if (float.IsNaN(_width)) IamNotAutoWidth();
                if (float.IsNaN(value)) IamAutoWidth();
                if (float.IsInfinity(value)) IamInfinyWidth(Parent);
                if (float.IsInfinity(_width)) IamNotInfinyWidth();
                _width = value;
                Validate();
            }
        }



        private float GetInnerWidth()
        {
            float x = 0;
            foreach (Region c in children)
                x = Max(c.Left + c.ActualWidth, x);
            return x;
        }
        private float GetInnerHeight()
        {
            float x = 0;
            foreach (Region c in children)
                x = Max(c.Top + c.ActualHeight, x);
            return x;
        }


        public float Height
        {
            get { return _height; }
            set
            {
                if (value == _height) return;
                if (float.IsNaN(_width))
                    foreach (Region c in children)
                        c.HeightChanged -= NaNControlChildHeightChanged;
                if (float.IsNaN(value))
                    foreach (Region c in children)
                        c.HeightChanged += NaNControlChildHeightChanged;
                _height = value;
                Validate();
            }
        }

        #endregion

        #region Actual Properties
        public float ActualLeft
        {
            get { return values.X; }
        }
        public float ActualTop
        {
            get { return values.Y; }
        }
        public float ActualWidth
        {
            get { return values.Width; }
        }
        public float ActualHeight
        {
            get { return values.Height; }
        }

        public SizeF ActualSize => new SizeF(values.Width, values.Height);
        public PointF ActualLocation => new PointF(values.X, values.Y);

        #endregion

        #region Events
        private event WidthChangedHandler WidthChanged;
        private event HeightChangedHandler HeightChanged;
        #endregion

        public Alignment Alignment
        {
            get { return _aligment; }
            set
            {
                if (_aligment == value) return;
                _aligment = value;
                Validate();
            }
        }

        private void ValidateSize()
        {
            var h = _height;
            if (float.IsInfinity(h) && Parent != null)
                values.Height = Parent.ActualHeight - (ActualTop - Parent.ActualTop);
            else if (float.IsNaN(h))
                values.Height = GetInnerHeight();
            else values.Height = h;
            var w = _width;
            if (float.IsInfinity(w) && Parent != null)
                values.Width = Parent.ActualWidth - (ActualLeft - Parent.ActualLeft);
            else if (float.IsNaN(w))
                values.Width = GetInnerWidth();
            else values.Width = w;
        }

        private void ValidateLocation()
        {
            var x = _left;
            var y = _top;
            var h = Alignment & Alignment.HCenter;
            var v = Alignment & Alignment.VCenter;

            if (h == Alignment.HCenter)
                x = (Parent.ActualWidth - ActualWidth) / 2;
            else if (h == Alignment.Right)
                x = Parent.ActualWidth - ActualWidth - x;

            if (v == Alignment.VCenter)
                y = (Parent.ActualHeight - ActualHeight) / 2;
            else if (v == Alignment.Bottom)
                y = Parent.ActualHeight - ActualHeight - y;

            values.Y = y + (Parent != this ? Parent.ActualTop : 0);
            values.X = x + (Parent != this ? Parent.ActualLeft : 0);
        }

        public bool Validate()
        {
            if (IsInitializing) return false;
            var t = values.GetControl();
            ValidateLocation();
            ValidateSize();
            return OnPropertyChanged(t);
        }

        public bool SetValues(float left, float top, float width, float height)
        {
            var t = values.GetControl();
            Left = left;
            Top = top;
            Width = width;
            Height = height;
            return OnPropertyChanged(t);
        }

        public Disposer BeginControl() => new Disposer(this);

        public class Disposer : IDisposable
        {
            private readonly object controler;
            private readonly Region region;

            internal Disposer(Region r)
            {
                region = r;
                controler = r.values.GetControl();
            }

            public void Dispose()
            {
                region.OnPropertyChanged(controler);
            }
            public bool IsOwn => region.values.IsControledBy(controler);
            ~Disposer()
            {
                Dispose();
            }
        }
    }
    public sealed partial class Region
    {
        public Region()
        {
            
        }
        private bool OnPropertyChanged(object version)
        {
            if (!values.IsControledBy(version)) return false;
            var changes = values.changes;
            var t = changes != 0;
            if (t)
            {
                PropertyChanged?.Invoke(this, values, changes);
                if ((changes & RP.Width) == RP.Width)
                    WidthChanged?.Invoke(this);
                if ((changes & RP.Height) == RP.Height)
                    HeightChanged?.Invoke(this);
            }
            values.CancelControl(version);
            return t;
        }
        public static RectangleF Intersect(RectangleF a, RPC b)
        {
            float x = Max(a.X, b.X);
            float num1 = Min(a.X + a.Width, b.X + b.Width);
            float y = Max(a.Y, b.Y);
            float num2 = Min(a.Y + a.Height, b.Y + b.Height);
            if (num1 >= (double)x && num2 >= (double)y)
                return new RectangleF(x, y, num1 - x, num2 - y);
            return RectangleF.Empty;
        }
        public static RectangleF Union(Region r1, RectangleF b)
        {
            var a = r1.values;
            float x = Min(a.X, b.X);
            float num1 = Max(a.X + a.Width, b.X + b.Width);
            float y = Min(a.Y, b.Y);
            float num2 = Max(a.Y + a.Height, b.Y + b.Height);
            return new RectangleF(x, y, num1 - x, num2 - y);
        }

        public static implicit operator RectangleF(Region r) 
            => new RectangleF(r.values.X, r.values.Y, r.values.Width, r.values.Height);

        public override string ToString() 
            => $"x:{values.X}, Y:{values.Y}, W:{values.Width}, H:{values.Height}";

        private readonly System.Collections.ArrayList children = new ArrayList();

        public bool IsInitializing => values.IsInitializing;
        public RectangleF DrawableRegion
        {
            get
            {
                return Parent == null ? this : Intersect(Parent.DrawableRegion, values);
            }
        }
    }

}