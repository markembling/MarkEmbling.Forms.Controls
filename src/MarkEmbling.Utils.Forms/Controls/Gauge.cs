using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace MarkEmbling.Utils.Forms.Controls {
    /*
     * Windows Forms Gauge control.
     * Very heavily based on/derived from AGauge by A.J.Bauer and improved on by Code Artist.
     */
    [DefaultEvent("ValueInRangeChanged"),
     Description("Displays a value on an analog gauge. Raises an event if the value enters one of the definable ranges.")]
    public partial class Gauge : Control {
        #region Private Fields

        private float _fontBoundY1;
        private float _fontBoundY2;
        private Bitmap _gaugeBitmap;
        private bool _drawGaugeBackground = true;

        private float _value;
        private Point _center = new Point(100, 100);
        private float _minValue = -100;
        private float _maxValue = 400;

        private Color _baseArcColour = Color.Gray;
        private int _baseArcRadius = 80;
        private int _baseArcStart = 135;
        private int _baseArcSweep = 270;
        private int _baseArcWidth = 2;

        private Color _scaleLinesInterColor = Color.Black;
        private int _scaleLinesInterInnerRadius = 73;
        private int _scaleLinesInterOuterRadius = 80;
        private int _scaleLinesInterWidth = 1;

        private int _scaleLinesMinorTicks = 9;
        private Color _scaleLinesMinorColor = Color.Gray;
        private int _scaleLinesMinorInnerRadius = 75;
        private int _scaleLinesMinorOuterRadius = 80;
        private int _scaleLinesMinorWidth = 1;

        private float _scaleLinesMajorStepValue = 50.0f;
        private Color _scaleLinesMajorColor = Color.Black;
        private int _scaleLinesMajorInnerRadius = 70;
        private int _scaleLinesMajorOuterRadius = 80;
        private int _scaleLinesMajorWidth = 2;

        private int _scaleNumbersRadius = 95;
        private Color _scaleNumbersColor = Color.Black;
        private string _scaleNumbersFormat;
        private int _scaleNumbersStartScaleLine;
        private int _scaleNumbersStepScaleLines = 1;
        private int _scaleNumbersRotation;

        private NeedleType _needleType;
        private int _needleRadius = 80;
        private Color _needleColor1 = Color.Gray;
        private Color _needleColor2 = Color.DimGray;
        private int _needleWidth = 2;

        #endregion

        #region EventHandler

        [Description("Raised when gauge value is changed.")]
        public event EventHandler ValueChanged;
        private void OnValueChanged() {
            EventHandler e = ValueChanged;
            if (e != null) e(this, null);
        }

        [Description("This event is raised if the value is entering or leaving defined range.")]
        public event EventHandler<ValueInRangeChangedEventArgs> ValueInRangeChanged;
        private void OnValueInRangeChanged(GaugeRange range, float value) {
            EventHandler<ValueInRangeChangedEventArgs> e = ValueInRangeChanged;
            if (e != null) e(this, new ValueInRangeChangedEventArgs(range, value, range.InRange));
        }

        #endregion

        #region Hidden and overridden inherited properties

        // Hide from designer
        // ReSharper disable ValueParameterNotUsed
        public new Boolean AllowDrop { get { return false; } set { /*Do Nothing */ } }
        public new Boolean AutoSize { get { return false; } set { /*Do Nothing */ } }
        public new Boolean ForeColor { get { return false; } set { /*Do Nothing */ } }
        public new Boolean ImeMode { get { return false; } set { /*Do Nothing */ } }
        // ReSharper restore ValueParameterNotUsed

        public override Color BackColor {
            get { return base.BackColor; }
            set {
                base.BackColor = value;
                _drawGaugeBackground = true;
                Refresh();
            }
        }

        public override Font Font {
            get { return base.Font; }
            set {
                base.Font = value;
                _drawGaugeBackground = true;
                Refresh();
            }
        }

        public override ImageLayout BackgroundImageLayout {
            get { return base.BackgroundImageLayout; }
            set {
                base.BackgroundImageLayout = value;
                _drawGaugeBackground = true;
                Refresh();
            }
        }

        #endregion

        public Gauge() {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            _gaugeRanges = new GaugeRangeCollection(this);
            _gaugeLabels = new GaugeLabelCollection(this);

            // Default Values
            Size = new Size(205, 180);
        }

        #region Properties

        [Browsable(true),
         Category("Gauge"),
         Description("Gauge value.")]
        public float Value {
            get { return _value; }
            set {
                value = Math.Min(Math.Max(value, _minValue), _maxValue);
                if (_value != value) {
                    _value = value;
                    OnValueChanged();

                    if (this.DesignMode) _drawGaugeBackground = true;

                    foreach (GaugeRange ptrRange in _gaugeRanges) {
                        if ((_value >= ptrRange.StartValue)
                            && (_value <= ptrRange.EndValue)) {
                            //Entering Range
                            if (!ptrRange.InRange) {
                                ptrRange.InRange = true;
                                OnValueInRangeChanged(ptrRange, _value);
                            }
                        } else {
                            //Leaving Range
                            if (ptrRange.InRange) {
                                ptrRange.InRange = false;
                                OnValueInRangeChanged(ptrRange, _value);
                            }
                        }
                    }
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("Gauge Ranges.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public GaugeRangeCollection GaugeRanges { get { return _gaugeRanges; } }
        private readonly GaugeRangeCollection _gaugeRanges;

        [Browsable(true),
        Category("Gauge"),
        Description("Gauge Labels.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public GaugeLabelCollection GaugeLabels { get { return _gaugeLabels; } }
        private readonly GaugeLabelCollection _gaugeLabels;

        #region << Gauge Base >>

        [Browsable(true),
         Category("Gauge"),
         Description("The center of the gauge (in the control's client area).")]
        public Point Center {
            get { return _center; }
            set {
                if (_center != value) {
                    _center = value;
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("The colour of the base arc.")]
        public Color BaseArcColour {
            get { return _baseArcColour; }
            set {
                if (_baseArcColour != value) {
                    _baseArcColour = value;
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("The radius of the base arc.")]
        public int BaseArcRadius {
            get { return _baseArcRadius; }
            set {
                if (_baseArcRadius != value) {
                    _baseArcRadius = value;
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("The start angle of the base arc.")]
        public int BaseArcStart {
            get { return _baseArcStart; }
            set {
                if (_baseArcStart != value) {
                    _baseArcStart = value;
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("The sweep angle of the base arc.")]
        public int BaseArcSweep {
            get { return _baseArcSweep; }
            set {
                if (_baseArcSweep != value) {
                    _baseArcSweep = value;
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("The width of the base arc.")]
        public int BaseArcWidth {
            get { return _baseArcWidth; }
            set {
                if (_baseArcWidth != value) {
                    _baseArcWidth = value;
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        #endregion

        #region << Gauge Scale >>

        [Browsable(true),
         Category("Gauge"),
         Description("The minimum value to show on the scale.")]
        public float MinValue {
            get { return _minValue; }
            set {
                if ((_minValue != value) && (value < _maxValue)) {
                    _minValue = value;
                    _value = Math.Min(Math.Max(_value, _minValue), _maxValue);
                    _scaleLinesMajorStepValue = Math.Min(_scaleLinesMajorStepValue, _maxValue - _minValue);
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("The maximum value to show on the scale.")]
        public float MaxValue {
            get { return _maxValue; }
            set {
                if ((_maxValue != value) && (value > _minValue)) {
                    _maxValue = value;
                    _value = Math.Min(Math.Max(_value, _minValue), _maxValue);
                    _scaleLinesMajorStepValue = Math.Min(_scaleLinesMajorStepValue, _maxValue - _minValue);
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("The color of the inter scale lines which are the middle scale lines for an uneven number of minor scale lines.")]
        public Color ScaleLinesInterColor {
            get { return _scaleLinesInterColor; }
            set {
                if (_scaleLinesInterColor != value) {
                    _scaleLinesInterColor = value;
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("The inner radius of the inter scale lines which are the middle scale lines for an uneven number of minor scale lines.")]
        public int ScaleLinesInterInnerRadius {
            get { return _scaleLinesInterInnerRadius; }
            set {
                if (_scaleLinesInterInnerRadius != value) {
                    _scaleLinesInterInnerRadius = value;
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("The outer radius of the inter scale lines which are the middle scale lines for an uneven number of minor scale lines.")]
        public int ScaleLinesInterOuterRadius {
            get { return _scaleLinesInterOuterRadius; }
            set {
                if (_scaleLinesInterOuterRadius != value) {
                    _scaleLinesInterOuterRadius = value;
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("The width of the inter scale lines which are the middle scale lines for an uneven number of minor scale lines.")]
        public int ScaleLinesInterWidth {
            get { return _scaleLinesInterWidth; }
            set {
                if (_scaleLinesInterWidth != value) {
                    _scaleLinesInterWidth = value;
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("The number of minor scale lines.")]
        public int ScaleLinesMinorTicks {
            get { return _scaleLinesMinorTicks; }
            set {
                if (_scaleLinesMinorTicks != value) {
                    _scaleLinesMinorTicks = value;
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("The color of the minor scale lines.")]
        public Color ScaleLinesMinorColor {
            get { return _scaleLinesMinorColor; }
            set {
                if (_scaleLinesMinorColor != value) {
                    _scaleLinesMinorColor = value;
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("The inner radius of the minor scale lines.")]
        public int ScaleLinesMinorInnerRadius {
            get { return _scaleLinesMinorInnerRadius; }
            set {
                if (_scaleLinesMinorInnerRadius != value) {
                    _scaleLinesMinorInnerRadius = value;
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("The outer radius of the minor scale lines.")]
        public int ScaleLinesMinorOuterRadius {
            get { return _scaleLinesMinorOuterRadius; }
            set {
                if (_scaleLinesMinorOuterRadius != value) {
                    _scaleLinesMinorOuterRadius = value;
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("The width of the minor scale lines.")]
        public int ScaleLinesMinorWidth {
            get { return _scaleLinesMinorWidth; }
            set {
                if (_scaleLinesMinorWidth != value) {
                    _scaleLinesMinorWidth = value;
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("The step value of the major scale lines.")]
        public float ScaleLinesMajorStepValue {
            get { return _scaleLinesMajorStepValue; }
            set {
                if ((_scaleLinesMajorStepValue != value) && (value > 0)) {
                    _scaleLinesMajorStepValue = Math.Min(value, _maxValue - _minValue);
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("The color of the major scale lines.")]
        public Color ScaleLinesMajorColor {
            get { return _scaleLinesMajorColor; }
            set {
                if (_scaleLinesMajorColor != value) {
                    _scaleLinesMajorColor = value;
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("The inner radius of the major scale lines.")]
        public int ScaleLinesMajorInnerRadius {
            get { return _scaleLinesMajorInnerRadius; }
            set {
                if (_scaleLinesMajorInnerRadius != value) {
                    _scaleLinesMajorInnerRadius = value;
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("The outer radius of the major scale lines.")]
        public int ScaleLinesMajorOuterRadius {
            get { return _scaleLinesMajorOuterRadius; }
            set {
                if (_scaleLinesMajorOuterRadius != value) {
                    _scaleLinesMajorOuterRadius = value;
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("The width of the major scale lines.")]
        public int ScaleLinesMajorWidth {
            get { return _scaleLinesMajorWidth; }
            set {
                if (_scaleLinesMajorWidth != value) {
                    _scaleLinesMajorWidth = value;
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        #endregion

        #region << Gauge Scale Numbers >>

        [Browsable(true),
         Category("Gauge"),
         Description("The radius of the scale numbers.")]
        public int ScaleNumbersRadius {
            get { return _scaleNumbersRadius; }
            set {
                if (_scaleNumbersRadius != value) {
                    _scaleNumbersRadius = value;
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("The color of the scale numbers.")]
        public Color ScaleNumbersColor {
            get { return _scaleNumbersColor; }
            set {
                if (_scaleNumbersColor != value) {
                    _scaleNumbersColor = value;
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("The format of the scale numbers.")]
        public string ScaleNumbersFormat {
            get { return _scaleNumbersFormat; }
            set {
                if (_scaleNumbersFormat != value) {
                    _scaleNumbersFormat = value;
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("The number of the scale line to start writing numbers next to.")]
        public int ScaleNumbersStartScaleLine {
            get { return _scaleNumbersStartScaleLine; }
            set {
                if (_scaleNumbersStartScaleLine != value) {
                    _scaleNumbersStartScaleLine = Math.Max(value, 1);
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("The number of scale line steps for writing numbers.")]
        public int ScaleNumbersStepScaleLines {
            get { return _scaleNumbersStepScaleLines; }
            set {
                if (_scaleNumbersStepScaleLines != value) {
                    _scaleNumbersStepScaleLines = Math.Max(value, 1);
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("The angle relative to the tangent of the base arc at a scale line that is used to rotate numbers. set to 0 for no rotation or e.g. set to 90.")]
        public int ScaleNumbersRotation {
            get { return _scaleNumbersRotation; }
            set {
                if (_scaleNumbersRotation != value) {
                    _scaleNumbersRotation = value;
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        #endregion

        #region << Gauge Needle >>

        [Browsable(true),
         Category("Gauge"),
         Description("The type of the needle drawn.")]
        public NeedleType NeedleType {
            get { return _needleType; }
            set {
                if (_needleType != value) {
                    _needleType = value;
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("The radius of the needle.")]
        public int NeedleRadius {
            get { return _needleRadius; }
            set {
                if (_needleRadius != value) {
                    _needleRadius = value;
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("The first colour of the needle.")]
        public Color NeedleColor1 {
            get { return _needleColor1; }
            set {
                if (_needleColor1 != value) {
                    _needleColor1 = value;
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("The second color of the needle.")]
        public Color NeedleColor2 {
            get { return _needleColor2; }
            set {
                if (_needleColor2 != value) {
                    _needleColor2 = value;
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
         Category("Gauge"),
         Description("The width of the needle.")]
        public int NeedleWidth {
            get { return _needleWidth; }
            set {
                if (_needleWidth != value) {
                    _needleWidth = value;
                    _drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        #endregion

        #endregion

        #region Helper

        private void FindFontBounds() {
            //find upper and lower bounds for numeric characters
            int c1;
            int c2;
            Boolean boundfound;
            Bitmap b;
            Graphics g;
            SolidBrush backBrush = new SolidBrush(Color.White);
            SolidBrush foreBrush = new SolidBrush(Color.Black);
            SizeF boundingBox;

            b = new Bitmap(5, 5);
            g = Graphics.FromImage(b);
            boundingBox = g.MeasureString("0123456789", Font, -1, StringFormat.GenericTypographic);
            b = new Bitmap((int)(boundingBox.Width), (int)(boundingBox.Height));
            g = Graphics.FromImage(b);
            g.FillRectangle(backBrush, 0.0F, 0.0F, boundingBox.Width, boundingBox.Height);
            g.DrawString("0123456789", Font, foreBrush, 0.0F, 0.0F, StringFormat.GenericTypographic);

            _fontBoundY1 = 0;
            _fontBoundY2 = 0;
            c1 = 0;
            boundfound = false;
            while ((c1 < b.Height) && (!boundfound)) {
                c2 = 0;
                while ((c2 < b.Width) && (!boundfound)) {
                    if (b.GetPixel(c2, c1) != backBrush.Color) {
                        _fontBoundY1 = c1;
                        boundfound = true;
                    }
                    c2++;
                }
                c1++;
            }

            c1 = b.Height - 1;
            boundfound = false;
            while ((0 < c1) && (!boundfound)) {
                c2 = 0;
                while ((c2 < b.Width) && (!boundfound)) {
                    if (b.GetPixel(c2, c1) != backBrush.Color) {
                        _fontBoundY2 = c1;
                        boundfound = true;
                    }
                    c2++;
                }
                c1--;
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

        public void RepaintControl() {
            _drawGaugeBackground = true;
            Refresh();
        }

        #endregion

        #region Base member overrides

        protected override void OnPaintBackground(PaintEventArgs pevent) {}

        protected override void OnPaint(PaintEventArgs e) {
            if ((Width < 10) || (Height < 10)) {
                return;
            }

            if (_drawGaugeBackground) {
                _drawGaugeBackground = false;

                FindFontBounds();

                _gaugeBitmap = new Bitmap(Width, Height, e.Graphics);
                Graphics ggr = Graphics.FromImage(_gaugeBitmap);
                ggr.FillRectangle(new SolidBrush(BackColor), ClientRectangle);

                if (BackgroundImage != null) {
                    switch (BackgroundImageLayout) {
                        case ImageLayout.Center:
                            ggr.DrawImageUnscaled(BackgroundImage, Width / 2 - BackgroundImage.Width / 2, Height / 2 - BackgroundImage.Height / 2);
                            break;
                        case ImageLayout.None:
                            ggr.DrawImageUnscaled(BackgroundImage, 0, 0);
                            break;
                        case ImageLayout.Stretch:
                            ggr.DrawImage(BackgroundImage, 0, 0, Width, Height);
                            break;
                        case ImageLayout.Tile:
                            int pixelOffsetX = 0;
                            int pixelOffsetY = 0;
                            while (pixelOffsetX < Width) {
                                pixelOffsetY = 0;
                                while (pixelOffsetY < Height) {
                                    ggr.DrawImageUnscaled(BackgroundImage, pixelOffsetX, pixelOffsetY);
                                    pixelOffsetY += BackgroundImage.Height;
                                }
                                pixelOffsetX += BackgroundImage.Width;
                            }
                            break;
                        case ImageLayout.Zoom:
                            if ((float)(BackgroundImage.Width / Width) < (float)(BackgroundImage.Height / Height)) {
                                ggr.DrawImage(BackgroundImage, 0, 0, Height, Height);
                            } else {
                                ggr.DrawImage(BackgroundImage, 0, 0, Width, Width);
                            }
                            break;
                    }
                }

                ggr.SmoothingMode = SmoothingMode.HighQuality;
                ggr.PixelOffsetMode = PixelOffsetMode.HighQuality;

                GraphicsPath gp = new GraphicsPath();
                float rangeStartAngle;
                float rangeSweepAngle;

                foreach (GaugeRange ptrRange in _gaugeRanges) {
                    if (ptrRange.EndValue > ptrRange.StartValue) {
                        rangeStartAngle = _baseArcStart + (ptrRange.StartValue - _minValue) * _baseArcSweep / (_maxValue - _minValue);
                        rangeSweepAngle = (ptrRange.EndValue - ptrRange.StartValue) * _baseArcSweep / (_maxValue - _minValue);
                        gp.Reset();
                        gp.AddPie(new Rectangle(_center.X - ptrRange.OuterRadius, _center.Y - ptrRange.OuterRadius,
                            2 * ptrRange.OuterRadius, 2 * ptrRange.OuterRadius), rangeStartAngle, rangeSweepAngle);
                        gp.Reverse();
                        gp.AddPie(new Rectangle(_center.X - ptrRange.InnerRadius, _center.Y - ptrRange.InnerRadius,
                            2 * ptrRange.InnerRadius, 2 * ptrRange.InnerRadius), rangeStartAngle, rangeSweepAngle);
                        gp.Reverse();
                        ggr.SetClip(gp);
                        ggr.FillPie(new SolidBrush(ptrRange.Colour), new Rectangle(_center.X - ptrRange.OuterRadius, _center.Y - ptrRange.OuterRadius, 2 * ptrRange.OuterRadius, 2 * ptrRange.OuterRadius), rangeStartAngle, rangeSweepAngle);
                    }
                }

                ggr.SetClip(ClientRectangle);
                if (_baseArcRadius > 0) {
                    ggr.DrawArc(new Pen(_baseArcColour, _baseArcWidth), new Rectangle(_center.X - _baseArcRadius, _center.Y - _baseArcRadius, 2 * _baseArcRadius, 2 * _baseArcRadius), _baseArcStart, _baseArcSweep);
                }

                string valueText = "";
                SizeF boundingBox;
                float countValue = 0;
                int counter1 = 0;
                while (countValue <= (_maxValue - _minValue)) {
                    valueText = (_minValue + countValue).ToString(_scaleNumbersFormat);
                    ggr.ResetTransform();
                    boundingBox = ggr.MeasureString(valueText, Font, -1, StringFormat.GenericTypographic);

                    gp.Reset();
                    gp.AddEllipse(new Rectangle(_center.X - _scaleLinesMajorOuterRadius, _center.Y - _scaleLinesMajorOuterRadius, 2 * _scaleLinesMajorOuterRadius, 2 * _scaleLinesMajorOuterRadius));
                    gp.Reverse();
                    gp.AddEllipse(new Rectangle(_center.X - _scaleLinesMajorInnerRadius, _center.Y - _scaleLinesMajorInnerRadius, 2 * _scaleLinesMajorInnerRadius, 2 * _scaleLinesMajorInnerRadius));
                    gp.Reverse();
                    ggr.SetClip(gp);

                    ggr.DrawLine(new Pen(_scaleLinesMajorColor, _scaleLinesMajorWidth),
                    (float)(Center.X),
                    (float)(Center.Y),
                    (float)(Center.X + 2 * _scaleLinesMajorOuterRadius * Math.Cos((_baseArcStart + countValue * _baseArcSweep / (_maxValue - _minValue)) * Math.PI / 180.0)),
                    (float)(Center.Y + 2 * _scaleLinesMajorOuterRadius * Math.Sin((_baseArcStart + countValue * _baseArcSweep / (_maxValue - _minValue)) * Math.PI / 180.0)));

                    gp.Reset();
                    gp.AddEllipse(new Rectangle(_center.X - _scaleLinesMinorOuterRadius, _center.Y - _scaleLinesMinorOuterRadius, 2 * _scaleLinesMinorOuterRadius, 2 * _scaleLinesMinorOuterRadius));
                    gp.Reverse();
                    gp.AddEllipse(new Rectangle(_center.X - _scaleLinesMinorInnerRadius, _center.Y - _scaleLinesMinorInnerRadius, 2 * _scaleLinesMinorInnerRadius, 2 * _scaleLinesMinorInnerRadius));
                    gp.Reverse();
                    ggr.SetClip(gp);

                    if (countValue < (_maxValue - _minValue)) {
                        for (int counter2 = 1; counter2 <= _scaleLinesMinorTicks; counter2++) {
                            if (((_scaleLinesMinorTicks % 2) == 1) && ((int)(_scaleLinesMinorTicks / 2) + 1 == counter2)) {
                                gp.Reset();
                                gp.AddEllipse(new Rectangle(_center.X - _scaleLinesInterOuterRadius, _center.Y - _scaleLinesInterOuterRadius, 2 * _scaleLinesInterOuterRadius, 2 * _scaleLinesInterOuterRadius));
                                gp.Reverse();
                                gp.AddEllipse(new Rectangle(_center.X - _scaleLinesInterInnerRadius, _center.Y - _scaleLinesInterInnerRadius, 2 * _scaleLinesInterInnerRadius, 2 * _scaleLinesInterInnerRadius));
                                gp.Reverse();
                                ggr.SetClip(gp);

                                ggr.DrawLine(new Pen(_scaleLinesInterColor, _scaleLinesInterWidth),
                                (float)(Center.X),
                                (float)(Center.Y),
                                (float)(Center.X + 2 * _scaleLinesInterOuterRadius * Math.Cos((_baseArcStart + countValue * _baseArcSweep / (_maxValue - _minValue) + counter2 * _baseArcSweep / (((float)((_maxValue - _minValue) / _scaleLinesMajorStepValue)) * (_scaleLinesMinorTicks + 1))) * Math.PI / 180.0)),
                                (float)(Center.Y + 2 * _scaleLinesInterOuterRadius * Math.Sin((_baseArcStart + countValue * _baseArcSweep / (_maxValue - _minValue) + counter2 * _baseArcSweep / (((float)((_maxValue - _minValue) / _scaleLinesMajorStepValue)) * (_scaleLinesMinorTicks + 1))) * Math.PI / 180.0)));

                                gp.Reset();
                                gp.AddEllipse(new Rectangle(_center.X - _scaleLinesMinorOuterRadius, _center.Y - _scaleLinesMinorOuterRadius, 2 * _scaleLinesMinorOuterRadius, 2 * _scaleLinesMinorOuterRadius));
                                gp.Reverse();
                                gp.AddEllipse(new Rectangle(_center.X - _scaleLinesMinorInnerRadius, _center.Y - _scaleLinesMinorInnerRadius, 2 * _scaleLinesMinorInnerRadius, 2 * _scaleLinesMinorInnerRadius));
                                gp.Reverse();
                                ggr.SetClip(gp);
                            } else {
                                ggr.DrawLine(new Pen(_scaleLinesMinorColor, _scaleLinesMinorWidth),
                                (float)(Center.X),
                                (float)(Center.Y),
                                (float)(Center.X + 2 * _scaleLinesMinorOuterRadius * Math.Cos((_baseArcStart + countValue * _baseArcSweep / (_maxValue - _minValue) + counter2 * _baseArcSweep / (((float)((_maxValue - _minValue) / _scaleLinesMajorStepValue)) * (_scaleLinesMinorTicks + 1))) * Math.PI / 180.0)),
                                (float)(Center.Y + 2 * _scaleLinesMinorOuterRadius * Math.Sin((_baseArcStart + countValue * _baseArcSweep / (_maxValue - _minValue) + counter2 * _baseArcSweep / (((float)((_maxValue - _minValue) / _scaleLinesMajorStepValue)) * (_scaleLinesMinorTicks + 1))) * Math.PI / 180.0)));
                            }
                        }
                    }

                    ggr.SetClip(ClientRectangle);

                    if (_scaleNumbersRotation != 0) {
                        ggr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                        ggr.RotateTransform(90.0F + _baseArcStart + countValue * _baseArcSweep / (_maxValue - _minValue));
                    }

                    ggr.TranslateTransform((float)(Center.X + _scaleNumbersRadius * Math.Cos((_baseArcStart + countValue * _baseArcSweep / (_maxValue - _minValue)) * Math.PI / 180.0f)),
                                           (float)(Center.Y + _scaleNumbersRadius * Math.Sin((_baseArcStart + countValue * _baseArcSweep / (_maxValue - _minValue)) * Math.PI / 180.0f)),
                                           System.Drawing.Drawing2D.MatrixOrder.Append);


                    if (counter1 >= ScaleNumbersStartScaleLine - 1) {
                        ggr.DrawString(valueText, Font, new SolidBrush(_scaleNumbersColor), -boundingBox.Width / 2, -_fontBoundY1 - (_fontBoundY2 - _fontBoundY1 + 1) / 2, StringFormat.GenericTypographic);
                    }

                    countValue += _scaleLinesMajorStepValue;
                    counter1++;
                }

                ggr.ResetTransform();
                ggr.SetClip(ClientRectangle);

                if (_scaleNumbersRotation != 0) {
                    ggr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
                }

                foreach (GaugeLabel ptrGaugeLabel in _gaugeLabels) {
                    if (!string.IsNullOrEmpty(ptrGaugeLabel.Text))
                        ggr.DrawString(ptrGaugeLabel.Text, ptrGaugeLabel.Font, new SolidBrush(ptrGaugeLabel.Color),
                            ptrGaugeLabel.Position.X, ptrGaugeLabel.Position.Y, StringFormat.GenericTypographic);
                }
            }

            e.Graphics.DrawImageUnscaled(_gaugeBitmap, 0, 0);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            float brushAngle = (int)(_baseArcStart + (_value - _minValue) * _baseArcSweep / (_maxValue - _minValue)) % 360;
            Double needleAngle = brushAngle * Math.PI / 180;

            switch (_needleType) {
                case NeedleType.ThreeDimensional:
                    var points = new PointF[3];
                    var brush1 = Brushes.White;
                    var brush2 = Brushes.White;
                    var brush3 = Brushes.White;
                    var brush4 = Brushes.White;

                    var brushBucket = Brushes.White;
                    var subcol = (int)(((brushAngle + 225) % 180) * 100 / 180);
                    var subcol2 = (int)(((brushAngle + 135) % 180) * 100 / 180);

                    e.Graphics.FillEllipse(new SolidBrush(_needleColor2), Center.X - _needleWidth * 3, Center.Y - _needleWidth * 3, _needleWidth * 6, _needleWidth * 6);
                    
                    // Create the brushes for the lines
                    brush1 = new SolidBrush(Color.FromArgb(80 + subcol, 80 + subcol, 80 + subcol));
                    brush2 = new SolidBrush(Color.FromArgb(180 - subcol, 180 - subcol, 180 - subcol));
                    brush3 = new SolidBrush(Color.FromArgb(80 + subcol2, 80 + subcol2, 80 + subcol2));
                    brush4 = new SolidBrush(Color.FromArgb(180 - subcol2, 180 - subcol2, 180 - subcol2));
                    e.Graphics.DrawEllipse(Pens.Gray, Center.X - _needleWidth * 3, Center.Y - _needleWidth * 3, _needleWidth * 6, _needleWidth * 6);

                    if (Math.Floor((float)(((brushAngle + 225) % 360) / 180.0)) == 0) {
                        brushBucket = brush1;
                        brush1 = brush2;
                        brush2 = brushBucket;
                    }

                    if (Math.Floor((float)(((brushAngle + 135) % 360) / 180.0)) == 0) {
                        brush4 = brush3;
                    }

                    points[0].X = (float)(Center.X + _needleRadius * Math.Cos(needleAngle));
                    points[0].Y = (float)(Center.Y + _needleRadius * Math.Sin(needleAngle));
                    points[1].X = (float)(Center.X - _needleRadius / 20 * Math.Cos(needleAngle));
                    points[1].Y = (float)(Center.Y - _needleRadius / 20 * Math.Sin(needleAngle));
                    points[2].X = (float)(Center.X - _needleRadius / 5 * Math.Cos(needleAngle) + _needleWidth * 2 * Math.Cos(needleAngle + Math.PI / 2));
                    points[2].Y = (float)(Center.Y - _needleRadius / 5 * Math.Sin(needleAngle) + _needleWidth * 2 * Math.Sin(needleAngle + Math.PI / 2));
                    e.Graphics.FillPolygon(brush1, points);

                    points[2].X = (float)(Center.X - _needleRadius / 5 * Math.Cos(needleAngle) + _needleWidth * 2 * Math.Cos(needleAngle - Math.PI / 2));
                    points[2].Y = (float)(Center.Y - _needleRadius / 5 * Math.Sin(needleAngle) + _needleWidth * 2 * Math.Sin(needleAngle - Math.PI / 2));
                    e.Graphics.FillPolygon(brush2, points);

                    points[0].X = (float)(Center.X - (_needleRadius / 20 - 1) * Math.Cos(needleAngle));
                    points[0].Y = (float)(Center.Y - (_needleRadius / 20 - 1) * Math.Sin(needleAngle));
                    points[1].X = (float)(Center.X - _needleRadius / 5 * Math.Cos(needleAngle) + _needleWidth * 2 * Math.Cos(needleAngle + Math.PI / 2));
                    points[1].Y = (float)(Center.Y - _needleRadius / 5 * Math.Sin(needleAngle) + _needleWidth * 2 * Math.Sin(needleAngle + Math.PI / 2));
                    points[2].X = (float)(Center.X - _needleRadius / 5 * Math.Cos(needleAngle) + _needleWidth * 2 * Math.Cos(needleAngle - Math.PI / 2));
                    points[2].Y = (float)(Center.Y - _needleRadius / 5 * Math.Sin(needleAngle) + _needleWidth * 2 * Math.Sin(needleAngle - Math.PI / 2));
                    e.Graphics.FillPolygon(brush4, points);

                    points[0].X = (float)(Center.X - _needleRadius / 20 * Math.Cos(needleAngle));
                    points[0].Y = (float)(Center.Y - _needleRadius / 20 * Math.Sin(needleAngle));
                    points[1].X = (float)(Center.X + _needleRadius * Math.Cos(needleAngle));
                    points[1].Y = (float)(Center.Y + _needleRadius * Math.Sin(needleAngle));

                    e.Graphics.DrawLine(new Pen(_needleColor2), Center.X, Center.Y, points[0].X, points[0].Y);
                    e.Graphics.DrawLine(new Pen(_needleColor2), Center.X, Center.Y, points[1].X, points[1].Y);
                    break;
                case NeedleType.Simple: 
                    var startPoint = new Point((int)(Center.X - _needleRadius / 8 * Math.Cos(needleAngle)),
                                                (int)(Center.Y - _needleRadius / 8 * Math.Sin(needleAngle)));
                    var endPoint = new Point((int)(Center.X + _needleRadius * Math.Cos(needleAngle)),
                                             (int)(Center.Y + _needleRadius * Math.Sin(needleAngle)));

                    e.Graphics.FillEllipse(new SolidBrush(_needleColor2), Center.X - _needleWidth * 3, Center.Y - _needleWidth * 3, _needleWidth * 6, _needleWidth * 6);

                    e.Graphics.DrawLine(new Pen(_needleColor1, _needleWidth), Center.X, Center.Y, endPoint.X, endPoint.Y);
                    e.Graphics.DrawLine(new Pen(_needleColor1, _needleWidth), Center.X, Center.Y, startPoint.X, startPoint.Y);

                    break;
            }
        }

        protected override void OnResize(EventArgs e) {
            _drawGaugeBackground = true;
            Refresh();
        }

        #endregion

    }



    /// <summary>
    /// Needle type for the Gauge control.
    /// </summary>
    public enum NeedleType {
        /// <summary>
        /// Simple solid line-style needle.
        /// </summary>
        Simple,

        /// <summary>
        /// 3D triangular needle.
        /// </summary>
        ThreeDimensional
    }







    public class GaugeRange {
        public GaugeRange() {}

        public GaugeRange(Color color, Single startValue, Single endValue, Int32 innerRadius, Int32 outerRadius) {
            Colour = color;
            _startValue = startValue;
            _endValue = endValue;
            InnerRadius = innerRadius;
            OuterRadius = outerRadius;
        }

        [Browsable(true),
         Category("Design"),
         DisplayName("(Name)"),
         Description("Instance Name.")]
        public string Name { get; set; }

        [Browsable(false)]
        public Boolean InRange { get; set; }

        private Gauge _owner;

        [Browsable(false)]
        public void SetOwner(Gauge value) {
            _owner = value;
        }

        private void NotifyOwner() {
            if (_owner != null) _owner.RepaintControl();
        }

        [Browsable(true),
         Category("Appearance"),
         Description("The colour of the range.")]
        public Color Colour {
            get { return _colour; }
            set {
                _colour = value;
                NotifyOwner();
            }
        }

        private Color _colour;

        [Browsable(true),
         Category("Limits"),
         Description("The start value of the range, must be less than RangeEndValue.")]
        public Single StartValue {
            get { return _startValue; }
            set {
                if (_owner != null) {
                    if (value < _owner.MinValue) value = _owner.MinValue;
                    if (value > _owner.MaxValue) value = _owner.MaxValue;
                }
                _startValue = value;
                NotifyOwner();
            }

        }

        private Single _startValue;

        [Browsable(true),
         Category("Limits"),
         Description("The end value of the range. Must be greater than RangeStartValue.")]
        public Single EndValue {
            get { return _endValue; }
            set {
                if (_owner != null) {
                    if (value < _owner.MinValue) value = _owner.MinValue;
                    if (value > _owner.MaxValue) value = _owner.MaxValue;
                }
                _endValue = value;
                NotifyOwner();
            }

        }

        private Single _endValue;

        [Browsable(true),
         Category("Appearance"),
         Description("The inner radius of the range.")]
        public Int32 InnerRadius {
            get { return _innerRadius; }
            set {
                if (value > 0) {
                    _innerRadius = value;
                    NotifyOwner();
                }
            }
        }

        private Int32 _innerRadius = 1;

        [Browsable(true),
         Category("Appearance"),
         Description("The outer radius of the range.")]
        public Int32 OuterRadius {
            get { return _outerRadius; }
            set {
                if (value > 0) {
                    _outerRadius = value;
                    NotifyOwner();
                }
            }
        }

        private Int32 _outerRadius = 2;
    }

    public class GaugeRangeCollection : CollectionBase {
        private readonly Gauge _owner;
        public GaugeRangeCollection(Gauge sender) { _owner = sender; }

        public GaugeRange this[int index] { get { return (GaugeRange)List[index]; } }
        public bool Contains(GaugeRange itemType) { return List.Contains(itemType); }

        public int Add(GaugeRange itemType) {
            itemType.SetOwner(_owner);
            if (string.IsNullOrEmpty(itemType.Name)) itemType.Name = GetUniqueName();
            return List.Add(itemType);
        }

        public void Remove(GaugeRange itemType) { List.Remove(itemType); }

        public void Insert(int index, GaugeRange itemType) {
            itemType.SetOwner(_owner);
            if (string.IsNullOrEmpty(itemType.Name)) itemType.Name = GetUniqueName();
            List.Insert(index, itemType);
        }

        public int IndexOf(GaugeRange itemType) { return List.IndexOf(itemType); }

        public GaugeRange FindByName(string name) {
            return List.Cast<GaugeRange>().FirstOrDefault(ptrRange => ptrRange.Name == name);
        }

        protected override void OnInsert(int index, object value) {
            if (string.IsNullOrEmpty(((GaugeRange)value).Name)) ((GaugeRange)value).Name = GetUniqueName();
            base.OnInsert(index, value);
            ((GaugeRange)value).SetOwner(_owner);
        }

        protected override void OnRemove(int index, object value) {
            if (_owner != null) _owner.RepaintControl();
        }

        protected override void OnClear() {
            if (_owner != null) _owner.RepaintControl();
        }

        private string GetUniqueName() {
            const string prefix = "GaugeRange";
            var index = 1;
            while (Count != 0) {
                var valid = true;
                for (var x = 0; x < Count; x++) {
                    if (this[x].Name == (prefix + index)) {
                        valid = false;
                        break;
                    }
                }
                if (valid) break;
                index++;
            };
            return prefix + index;
        }
    }



    public class GaugeLabel {
        [Browsable(true),
         Category("Design"),
         DisplayName("(Name)"),
         Description("Instance Name.")]
        public string Name { get; set; }

        private Gauge _owner;
        [Browsable(false)]
        public void SetOwner(Gauge value) { _owner = value; }
        private void NotifyOwner() { if (_owner != null) _owner.RepaintControl(); }

        [Browsable(true),
         Category("Appearance"),
         Description("The colour of the caption text.")]
        public Color Color { get { return _color; } set { _color = value; NotifyOwner(); } }
        private Color _color = Color.FromKnownColor(KnownColor.WindowText);

        [Browsable(true),
         Category("Appearance"),
         Description("The text of the caption.")]
        public String Text { get { return _text; } set { _text = value; NotifyOwner(); } }
        private String _text;

        [Browsable(true),
         Category("Appearance"),
         Description("The position of the caption.")]
        public Point Position { get { return _position; } set { _position = value; NotifyOwner(); } }
        private Point _position;

        [Browsable(true),
         Category("Appearance"),
         Description("Font of Text.")]
        public Font Font { get { return _font; } set { _font = value; NotifyOwner(); } }
        private Font _font = DefaultFont;

        public void ResetFont() { _font = DefaultFont; }
        private Boolean ShouldSerializeFont() { return (_font != DefaultFont); }
        private static readonly Font DefaultFont = Control.DefaultFont;
    }

    public class GaugeLabelCollection : CollectionBase {
        private readonly Gauge _owner;
        public GaugeLabelCollection(Gauge sender) { _owner = sender; }

        public GaugeLabel this[int index] { get { return (GaugeLabel)List[index]; } }

        public bool Contains(GaugeLabel itemType) { return List.Contains(itemType); }

        public int Add(GaugeLabel itemType) {
            itemType.SetOwner(_owner);
            if (string.IsNullOrEmpty(itemType.Name)) itemType.Name = GetUniqueName();
            return List.Add(itemType);
        }

        public void Remove(GaugeLabel itemType) { List.Remove(itemType); }

        public void Insert(int index, GaugeLabel itemType) {
            itemType.SetOwner(_owner);
            if (string.IsNullOrEmpty(itemType.Name)) itemType.Name = GetUniqueName();
            List.Insert(index, itemType);
        }

        public int IndexOf(GaugeLabel itemType) { return List.IndexOf(itemType); }

        public GaugeLabel FindByName(string name) {
            return List.Cast<GaugeLabel>().FirstOrDefault(ptrRange => ptrRange.Name == name);
        }

        protected override void OnInsert(int index, object value) {
            if (string.IsNullOrEmpty(((GaugeLabel)value).Name)) ((GaugeLabel)value).Name = GetUniqueName();
            base.OnInsert(index, value);
            ((GaugeLabel)value).SetOwner(_owner);
        }

        protected override void OnRemove(int index, object value) {
            if (_owner != null) _owner.RepaintControl();
        }

        protected override void OnClear() {
            if (_owner != null) _owner.RepaintControl();
        }

        private string GetUniqueName() {
            const string prefix = "GaugeLabel";
            var index = 1;
            while (Count != 0) {
                for (var x = 0; x < Count; x++) {
                    if (this[x].Name == (prefix + index))
                        continue;
                    return prefix + index;
                }
                index++;
            };
            return prefix + index;
        }
    }


    public class ValueInRangeChangedEventArgs : EventArgs {
        /// <summary>
        /// GaugeRange affected by this event.
        /// </summary>
        public GaugeRange Range { get; private set; }

        /// <summary>
        /// Gauge value
        /// </summary>
        public float Value { get; private set; }

        /// <summary>
        /// Whether or not the value is within this range (have we entered it or left it?)
        /// </summary>
        public bool InRange { get; private set; }

        public ValueInRangeChangedEventArgs(GaugeRange range, Single value, bool inRange) {
            Range = range;
            Value = value;
            InRange = inRange;
        }
    }
}