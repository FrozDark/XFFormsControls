using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace XFFormsControls.Controls
{
    [ContentProperty(nameof(Content))]
    public class SideMenu : Layout<View>
    {
        #region Views
        private View _leftView;
        public View LeftView
        {
            get => _leftView;
            set
            {
                if (_leftView != value)
                {
                    if (!(_leftView is null))
                    {
                        Children.Remove(_leftView);
                    }
                    _leftView = value;
                    Children.Add(value);
                }
            }
        }

        private View _topView;
        public View TopView
        {
            get => _topView;
            set
            {
                if (_topView != value)
                {
                    if (!(_topView is null))
                    {
                        Children.Remove(_topView);
                    }
                    _topView = value;
                    Children.Add(value);
                }
            }
        }

        private View _rightView;
        public View RightView
        {
            get => _rightView;
            set
            {
                if (_rightView != value)
                {
                    if (!(_rightView is null))
                    {
                        Children.Remove(_rightView);
                    }
                    _rightView = value;
                    Children.Add(value);
                }
            }
        }

        private View _bottomView;
        public View BottomView
        {
            get => _bottomView;
            set
            {
                if (_bottomView != value)
                {
                    if (!(_bottomView is null))
                    {
                        Children.Remove(_bottomView);
                    }
                    _bottomView = value;
                    Children.Add(value);
                }
            }
        }

        private View _content;
        public View Content
        {
            get => _content;
            set
            {
                if (_content != value)
                {
                    if (!(_content is null))
                    {
                        Children.Remove(_content);
                    }
                    _content = value;
                    Children.Insert(0, value);
                }
            }
        }
        #endregion

        public SideMenu()
        {
        }

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            return new SizeRequest(new Size(widthConstraint, heightConstraint), new Size(40.0, 40.0));
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);


        }

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            if (Content != null)
            {
                LayoutChildIntoBoundingRegion(Content, new Rectangle(x, y, width, height));
            }
        }
    }
}
