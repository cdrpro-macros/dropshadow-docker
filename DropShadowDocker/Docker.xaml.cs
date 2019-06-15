using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Xml;
using Corel.Interop.VGCore;
using Microsoft.Win32;
using Point = System.Windows.Point;
using Color = System.Windows.Media.Color;
using Control = System.Windows.Controls.Control;

namespace DropShadow
{
    public partial class Docker : UserControl
    {
        public static Corel.Interop.VGCore.Application DApp = null;

        public const string MName = "DropShadowDocker";
        public const string MVer = "1.3";
        public const string MDate = "08.04.2014";
        public const string MWebSite = @"https://cdrpro.ru";
        public const string MWebPage = @"https://cdrpro.ru/en/macros/dropshadow/";
        public const string MEmail = "info@cdrpro.ru";

        public static string CurLangFile = @"\Default.xml";

        private static List<DropShadowPreset> _presets;

        private bool _isMove = false;
        const double Rad2Deg = 180.0 / Math.PI;

        private Corel.Interop.VGCore.Color _shColor = null;
        private string _uPath;
        public static string ULangPath;

        public static string InputStr = "";

        private bool _byHand = false;
        private bool _angleNotUpdate = false;
        private XmlDocument _xHelp = null;
        private ShapeRange _selectedShapeRange = null;

        public Docker() { InitializeComponent(); }

        public Docker(object app)
        {
            try
            {
                InitializeComponent();
                DApp = (Corel.Interop.VGCore.Application)app;

                Load1();

                cbMode.Items.Add("Normal");
                cbMode.Items.Add("Multiply");
                cbMode.Items.Add("Add");

                MouseUp += new MouseButtonEventHandler(MainWindow_MouseUp);
                MouseMove += new MouseEventHandler(MainWindow_MouseMove);

                //if (!File.Exists(uPath)) CreateXml();
                LoadSettings();
                LoadLangMenu();

                applyColorForUI();
                LoadPresets();

                /* load help */
                _xHelp = new XmlDocument();
                _xHelp.Load(ULangPath);
                /* load help */

            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Load1()
        {
            try
            {
                string uFolderPath = Environment.GetEnvironmentVariable("APPDATA") + @"\Corel\" + MName;
                if (!Directory.Exists(uFolderPath)) Directory.CreateDirectory(uFolderPath);
                _uPath = uFolderPath + @"\Settings.xml";

                if (!File.Exists(_uPath)) CreateXml();

                /* lang ============================================================ */
                ULangPath = uFolderPath + CurLangFile;
                if (!File.Exists(ULangPath)) UploadLangFile(uFolderPath, "DefaultLanguage", "Default");

                //other languages
                var uRusLangPath = uFolderPath + @"\1049.xml";
                if (!File.Exists(uRusLangPath)) UploadLangFile(uFolderPath, "Languages.1049", "1049");

                var uTurLangPath = uFolderPath + @"\1055.xml";
                if (!File.Exists(uTurLangPath)) UploadLangFile(uFolderPath, "Languages.1055", "1055");
                //other languages

                UpdateFiles(); // update files

                string uiLng = @"\" + DApp.UILanguage.GetHashCode().ToString() + @".xml";
                if (File.Exists(uFolderPath + uiLng)) CurLangFile = uiLng;


                var key = Registry.CurrentUser.OpenSubKey("Software\\CDRPRO MACROS\\" + Docker.MName);
                if (key != null)
                {
                    var sKey = (string)key.GetValue("Lang", "");
                    key.Close();
                    if (sKey != "") CurLangFile = @"\" + sKey + @".xml";
                }

                ULangPath = uFolderPath + CurLangFile;
                LoadLang(this, "Lang");
                /* lang ============================================================ */
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UploadLangFile(string uFolderPath, string name, string fileName)
        {
            try
            {
                var uCostLangPath = uFolderPath + @"\" + fileName + @".xml";
                Stream inFile = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("DropShadow." + name + ".xml");
                var xDocDef = new XmlDocument();
                xDocDef.Load(inFile);
                xDocDef.Save(uCostLangPath);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateFiles()
        {
            try
            {
                //if (!File.Exists(uPath)) return;

                string uFolderPath = Environment.GetEnvironmentVariable("APPDATA") + @"\Corel\" + MName;

                var xDoc = new XmlDocument();
                xDoc.Load(_uPath);

                var inShadow = xDoc.SelectSingleNode(@"/App/Options/InnerShadow");
                if (inShadow == null)
                {
                    UploadLangFile(uFolderPath, "DefaultLanguage", "Default");
                    UploadLangFile(uFolderPath, "Languages.1049", "1049");
                    UploadLangFile(uFolderPath, "Languages.1055", "1055");

                    var opt = xDoc.SelectSingleNode(@"/App/Options");
                    var nn = xDoc.CreateElement("InnerShadow");
                    nn.InnerText = "0";
                    opt.AppendChild(nn);
                    xDoc.Save(_uPath);
                }

            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadLangMenu()
        {
            try
            {
                string uFolderPath = Environment.GetEnvironmentVariable("APPDATA") + @"\Corel\" + MName;
                var xDoc = new XmlDocument();
                xDoc.Load(_uPath);
                foreach (XmlNode n in xDoc.SelectSingleNode(@"/App/Languages").ChildNodes)
                {
                    var id = n.Attributes["Id"].Value;
                    var uAddLangPath = uFolderPath + @"\" + id + @".xml";
                    if (File.Exists(uAddLangPath))
                    {
                        var mi = new MenuItem { Header = n.Attributes["Name"].Value, Tag = id };
                        mi.Click += new RoutedEventHandler(ChangeLang);
                        LangMenu.Items.Add(mi);
                    }
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void LoadLang(FrameworkElement obj, string Key)
        {
            var xd = (XmlDataProvider)obj.Resources[Key];
            xd.Source = new Uri(ULangPath);
        }

        private void CreateXml()
        {
            Stream inFile = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("DropShadow.DefaultSettings.xml");
            var xDoc = new XmlDocument(); xDoc.Load(inFile); xDoc.Save(_uPath);
        }

        private void LoadSettings()
        {
            var xDoc = new XmlDocument();
            xDoc.Load(_uPath);

            _byHand = true;
            foreach (XmlNode n in xDoc.SelectSingleNode(@"/App/Options").ChildNodes)
            {
                switch (n.Name)
                {
                    case "BlendMode": cbMode.SelectedValue = n.InnerText; break;
                    case "Color": _shColor = DApp.CreateColor(n.InnerText); break;
                    case "Opacity": tbOpacity.Text = n.InnerText; break;
                    case "Angle": tbAngle.Text = n.InnerText; break;
                    case "Distance": tbDistance.Text = n.InnerText; break;
                    case "Size": tbSize.Text = n.InnerText; break;
                    case "Feather": tbFeather.Text = n.InnerText; break;
                    case "DPI": tbDPI.Text = n.InnerText; break;
                    case "UseDocumentDPI": cbDPI.IsChecked = ValToBool(n.InnerText); break;
                    case "GroupShadowWithParent": cbGroup.IsChecked = ValToBool(n.InnerText); break;
                    case "ShadowOverPrint": cbOverPrint.IsChecked = ValToBool(n.InnerText); break;
                    case "BitmapToPowerClip": cbBitmapToPowerClip.IsChecked = ValToBool(n.InnerText); break;
                    case "InnerShadow": cbInnerShadow.IsChecked = ValToBool(n.InnerText); break;
                    case "exOptions": exOptions.IsExpanded = ValToBool(n.InnerText); break;
                    case "exPresets": exPresets.IsExpanded = ValToBool(n.InnerText); break;
                    case "exHelp": exHelp.IsExpanded = ValToBool(n.InnerText); break;
                }
            }
            _byHand = false;
        }

        private bool ValToBool(string val) { return val == "1" ? true : false; }
        private string BoolToVal(bool b) { return b == true ? "1" : "0"; }

        private void CreateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DApp.Documents.Count == 0) return;

            try
            {
                Document doc = DApp.ActiveDocument;

                if (DApp.ActiveSelectionRange.Count == 0) return;
                doc.Unit = doc.Rulers.HUnits;

                /* get settings */
                int dpi = (bool)cbDPI.IsChecked == true ? doc.Resolution : Convert.ToInt32(tbDPI.Text);
                double angle = str2dbl(tbAngle.Text);
                double distance = str2dbl(tbDistance.Text);
                Point p = getPoint(0, 0, angle, distance);
                double size = str2dbl(tbSize.Text);
                /* get settings */

                Shape sh = null;
                Shape target = null;
                ShapeRange sr = null;

                boostStart("Create Shadow");
                _selectedShapeRange = DApp.ActiveSelectionRange;

                foreach (Shape s in DApp.ActiveSelectionRange)
                {
                    if (!s.Layer.Printable) s.Layer.Printable = true;

                    if ((bool)cbInnerShadow.IsChecked)
                    {
                        switch (s.Type.ToString())
                        {
                            case "cdrCurveShape":
                            case "cdrEllipseShape":
                            case "cdrTextShape":
                            case "cdrPolygonShape":
                            case "cdrPerfectShape":
                            case "cdrRectangleShape":
                                target = s;
                                sh = s.Duplicate(p.X, p.Y);
                                if (sh.PowerClip != null) sh.PowerClip.Shapes.All().Delete();
                                break;
                            case "cdrBitmapShape":
                                sr = traceBitmap(s, p);
                                if (sr.Count < 2) sh = null;
                                else { sh = sr[1]; target = sr[2]; }
                                break;
                            default: sh = null; MessageBox.Show("Not supported type"); break;
                        }
                    }
                    else
                    {
                        switch (s.Type.ToString())
                        {
                            case "cdrCurveShape":
                            case "cdrEllipseShape":
                            case "cdrTextShape":
                            case "cdrGroupShape":
                            case "cdrPolygonShape":
                            case "cdrPerfectShape":
                            case "cdrRectangleShape":
                            case "cdrMeshFillShape":
                                target = s;
                                sh = s.Duplicate(p.X, p.Y);
                                if (sh.PowerClip != null) sh.PowerClip.Shapes.All().Delete();
                                break;
                            case "cdrBitmapShape":
                                sr = traceBitmap(s, p);
                                if (sr.Count == 0) sh = null;
                                else if (sr.Count == 1) { sh = sr[1]; target = s; }
                                else { sh = sr[1]; target = sr[2]; }
                                break;
                            default: sh = null; MessageBox.Show("Not supported type"); break;
                        }
                    }

                    if (sh != null) createShadow(target, sh, dpi, angle, size);

                }

                doc.ClearSelection();
                boostFinish();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error);
                boostFinish();
            }
        }

        private void UndoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DApp.Documents.Count == 0) return;
            try
            {
                DApp.ActiveDocument.Undo(1);
                if (_selectedShapeRange != null)
                {
                    DApp.ActiveDocument.ClearSelection();
                    _selectedShapeRange.AddToSelection();
                    _selectedShapeRange = null;
                }
            }
            catch (Exception err) { MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private ShapeRange traceBitmap(Shape s, Point p)
        {
            var sr = new ShapeRange();
            sr.RemoveAll();

            try
            {
                Shape dub = s.Duplicate(p.X, p.Y);
                dub.Bitmap.Resample(0, 0, true, 72, 72);
                dub.ApplyEffectBCI(-100, 100, -100);

                ShapeRange tr = dub.Bitmap.Trace(
                    cdrTraceType.cdrTraceLineArt, -1, 1,
                    cdrColorType.cdrColorCMYK,
                    cdrPaletteID.cdrCustom, 1, true, false, true
                    ).Finish();

                Shape path = tr.UngroupAllEx().Combine();
                path.Curve.Nodes.All().AutoReduce(0.01);
                sr.Add(path);

                if ((bool)cbBitmapToPowerClip.IsChecked)
                {
                    Shape clip = path.Duplicate(p.X * -1, p.Y * -1);
                    clip.OrderBackOf(s);
                    clip.Fill.ApplyNoFill();
                    clip.Outline.SetNoOutline();
                    s.AddToPowerClip(clip);
                    sr.Add(clip);
                }

                return sr;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error);
                return sr;
            }

        }

        private void createShadow(Shape s, Shape sh, int dpi, double angle, double size)
        {
            try
            {
                ShapeRange forComb = null;
                double tw = 0, th = 0, tx = 0, ty = 0;

                if ((bool)cbInnerShadow.IsChecked)
                {
                    double nw = 0, nh = 0, nx = 0, ny = 0;

                    sh.GetSize(out nw, out nh);
                    sh.GetPositionEx(cdrReferencePoint.cdrCenter, out nx, out ny);

                    //double dist = str2dbl(tbDistance.Text);
                    //if (dist == 0) dist = size;
                    //if (dist == 0) dist = 0.1;
                    //nw = nw + (dist * 4); nh = nh + (dist * 4);

                    nw = nw + (nw / 2); nh = nh + (nh / 2);

                    Shape sAround = sh.Layer.CreateRectangle2(0, 0, nw, nh, 0, 0, 0, 0);
                    sAround.SetPositionEx(cdrReferencePoint.cdrCenter, nx, ny);

                    forComb = new ShapeRange();
                    forComb.Add(sh);
                    forComb.Add(sAround);
                    sh = forComb.Combine();
                }

                sh.OrderBackOf(s);

                sh.Fill.UniformColor.CopyAssign(_shColor);
                sh.Outline.SetProperties(size, null, _shColor);

                if ((bool)cbInnerShadow.IsChecked) sh.FillMode = cdrFillMode.cdrFillAlternate; //new fill fix

                var imgType = cdrImageType.cdrCMYKColorImage;
                switch (_shColor.Type.ToString())
                {
                    case "cdrColorGray": imgType = cdrImageType.cdrGrayscaleImage; break;
                    case "cdrColorLab": imgType = cdrImageType.cdrLABImage; break;
                    case "cdrColorRGB": imgType = cdrImageType.cdrRGBColorImage; break;
                }

                sh = sh.ConvertToBitmapEx(imgType, false, true, dpi, cdrAntiAliasingType.cdrNormalAntiAliasing, true, false);

                double feather = str2dbl(tbFeather.Text) * 100;
                // TODO: Looks like ApplyBitmapEffect doesn't work correctly in 2019
                sh.Bitmap.ApplyBitmapEffect(@"Gaussian Blur", @"GaussianBlurEffect GaussianBlurRadius=" + feather.ToString(CultureInfo.InvariantCulture) + ", GaussianBlurResampled=0");

                int opacity = (Convert.ToInt32(tbOpacity.Text) - 100) * -1;
                sh.Transparency.ApplyUniformTransparency(opacity);
                sh.Transparency.AppliedTo = cdrTransparencyAppliedTo.cdrApplyToFillAndOutline;

                switch (cbMode.SelectedValue.ToString())
                {
                    case "Normal": sh.Transparency.MergeMode = cdrMergeMode.cdrMergeNormal; break;
                    case "Multiply": sh.Transparency.MergeMode = cdrMergeMode.cdrMergeMultiply; break;
                    case "Add": sh.Transparency.MergeMode = cdrMergeMode.cdrMergeAdd; break;
                        //case "0": sh.Transparency.MergeMode = 
                }

                sh = sh.ConvertToBitmapEx(imgType, false, true, dpi, cdrAntiAliasingType.cdrNormalAntiAliasing, true, false);

                sh.Properties["DS2", 1] = createPresetStr(); //add settings
                if ((bool)cbOverPrint.IsChecked) sh.OverprintBitmap = true;

                if ((bool)cbInnerShadow.IsChecked)
                {
                    sh.AddToPowerClip(s);
                }
                else
                {
                    if ((bool)cbGroup.IsChecked)
                    {
                        var gr = new ShapeRange();
                        gr.Add(s); gr.Add(sh);
                        gr.Group();
                        gr = null;
                    }
                }

            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /* Load presets */
        private void LoadPresets()
        {
            _presets = new List<DropShadowPreset>();

            var xDoc = new XmlDocument();
            xDoc.Load(_uPath);
            foreach (XmlNode n in xDoc.SelectSingleNode(@"/App/Presets").ChildNodes)
            {
                _presets.Add(new DropShadowPreset(n.Attributes["Name"].Value, n.Attributes["Value"].Value));
            }

            PresetsList.ItemsSource = _presets;
            PresetsList.Items.Refresh();
        }

        /* Load preset */
        private void loadBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DApp.Documents.Count == 0) return;
                if (DApp.ActiveSelectionRange.Count == 0) return;

                string opt = DApp.ActiveSelectionRange[1].Properties["DS2", 1];
                if (opt != null)
                {
                    if (opt.Length == 0) return;
                    LoadPreset(opt);
                    SaveAllSettings();
                }
                else
                {
                    // try load old settings
                    opt = DApp.ActiveSelectionRange[1].Properties["DS", 1];
                    if (opt == null) return;
                    if (opt.Length == 0) return;
                    LoadOldPreset(opt);
                    SaveAllSettings();
                }

            }
            catch (Exception err) { MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        /* Add preset */
        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            var ib = new InputBox();
            var wih = new System.Windows.Interop.WindowInteropHelper(ib);
            wih.Owner = (IntPtr)DApp.AppWindow.Handle;
            ib.ShowDialog();
            if (InputStr.Length == 0) return;

            var xDoc = new XmlDocument();
            xDoc.Load(_uPath);
            XmlNode pn = xDoc.SelectSingleNode(@"/App/Presets");

            XmlElement pr = xDoc.CreateElement("Preset");
            pr.SetAttribute("Name", InputStr);
            pr.SetAttribute("Value", createPresetStr());

            if (pn.ChildNodes.Count == 0) pn.AppendChild(pr);
            else pn.InsertAfter(pr, pn.LastChild);

            xDoc.Save(_uPath);
            LoadPresets();
            InputStr = "";
        }

        /* Apply preset */
        private void PresetsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (PresetsList.SelectedIndex == -1) return;
            var p = (DropShadowPreset)PresetsList.SelectedItem;

            if (p.value.Length != 0)
            {
                LoadPreset(p.value);
                SaveAllSettings();
            }
        }

        /* Save preset */
        private void SavePreset_Click(object sender, RoutedEventArgs e)
        {
            if (PresetsList.SelectedIndex == -1) return;
            var p = (DropShadowPreset)PresetsList.SelectedItem;

            InputStr = p.name;
            var ib = new InputBox();
            var wih = new System.Windows.Interop.WindowInteropHelper(ib) { Owner = (IntPtr)DApp.AppWindow.Handle };
            ib.ShowDialog();
            if (InputStr.Length == 0) return;

            var xDoc = new XmlDocument();
            xDoc.Load(_uPath);
            var d = xDoc.SelectSingleNode("//Preset[@Name = \"" + p.name + "\"]");

            if (d != null)
            {
                var pn = xDoc.SelectSingleNode(@"/App/Presets");

                var pr = xDoc.CreateElement("Preset");
                pr.SetAttribute("Name", InputStr);
                pr.SetAttribute("Value", createPresetStr());

                pn.ReplaceChild(pr, d);
                xDoc.Save(_uPath);
                LoadPresets();
                InputStr = "";
                return;
            }

            MessageBox.Show("Item not found!");
        }

        /* Delete preset */
        private void DeletePreset_Click(object sender, RoutedEventArgs e)
        {
            if (PresetsList.SelectedIndex == -1) return;
            var p = (DropShadowPreset)PresetsList.SelectedItem;

            var xDoc = new XmlDocument();
            xDoc.Load(_uPath);
            var d = xDoc.SelectSingleNode("//Preset[@Name = \"" + p.name + "\"]");

            if (d != null)
            {
                var pn = xDoc.SelectSingleNode(@"/App/Presets");
                pn.RemoveChild(d);
                xDoc.Save(_uPath);
                LoadPresets();
                return;
            }

            MessageBox.Show("Item not found!");
        }

        private string createPresetStr()
        {
            return cbMode.SelectedValue.ToString() + @"|" +
                _shColor.ToString() + @"|" +
                tbOpacity.Text + @"|" +
                tbAngle.Text + @"|" +
                tbDistance.Text + @"|" +
                tbSize.Text + @"|" +
                tbFeather.Text + @"|" +
                tbDPI.Text + @"|" +
                BoolToVal((bool)cbDPI.IsChecked) + @"|" +
                BoolToVal((bool)cbGroup.IsChecked) + @"|" +
                BoolToVal((bool)cbOverPrint.IsChecked) + @"|" +
                BoolToVal((bool)cbBitmapToPowerClip.IsChecked) + @"|" +
                BoolToVal((bool)cbInnerShadow.IsChecked);
        }

        private void LoadPreset(string opt)
        {
            try
            {
                _byHand = true;

                string[] opts = opt.Split('|');

                cbMode.SelectedValue = opts[0];
                _shColor = DApp.CreateColor(opts[1]); applyColorForUI();
                tbOpacity.Text = opts[2];
                tbAngle.Text = opts[3];
                tbDistance.Text = opts[4];
                tbSize.Text = opts[5];
                tbFeather.Text = opts[6];
                tbDPI.Text = opts[7];
                cbDPI.IsChecked = ValToBool(opts[8]);
                cbGroup.IsChecked = ValToBool(opts[9]);
                cbOverPrint.IsChecked = ValToBool(opts[10]);
                cbBitmapToPowerClip.IsChecked = ValToBool(opts[11]);
                cbInnerShadow.IsChecked = false;

                if (opts.Length == 13) cbInnerShadow.IsChecked = ValToBool(opts[12]);

                _byHand = false;
            }
            catch (Exception err) { MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error); _byHand = false; }
        }

        private void LoadOldPreset(string opt)
        {
            try
            {
                _byHand = true;

                string from = "";
                string to = "";

                var key = Registry.CurrentUser.OpenSubKey("Software\\CDRPRO MACROS\\" + Docker.MName);
                if (key != null)
                {
                    var uFrom = (string)key.GetValue("UnitsFrom", "");
                    if (uFrom != "") from = uFrom;
                    var uTo = (string)key.GetValue("UnitsTo", "");
                    if (uTo != "") to = uTo;
                    key.Close();
                }

                string[] opts = opt.Split('|');

                cbMode.SelectedValue = opts[5];

                DecodeOldColor(opts[13]);
                applyColorForUI();

                tbOpacity.Text = (100 - Convert.ToInt32(opts[4])).ToString(CultureInfo.InvariantCulture);

                string x = opts[0];
                string y = opts[1];
                string sz = opts[2];

                if (from != "" && to != "")
                {
                    x = ConvertUnit(x, from, to).ToString(CultureInfo.InvariantCulture);
                    y = ConvertUnit(y, from, to).ToString(CultureInfo.InvariantCulture);
                    sz = Math.Round(ConvertUnit(sz, from, to), 2).ToString(CultureInfo.InvariantCulture);
                }

                var cp = new Point(0.0, 0.0);
                var cCur = new Point(str2dbl(x), (str2dbl(y) * -1));
                var angle = Angle(cp, cCur);
                var val = GetDrawAngle(Math.Round(angle)).ToString(CultureInfo.InvariantCulture);
                tbAngle.Text = val;

                double dist = Math.Round(Distance2D(cp, cCur), 2);
                tbDistance.Text = dist.ToString(CultureInfo.InvariantCulture);

                tbSize.Text = sz;
                tbFeather.Text = opts[3];
                tbDPI.Text = "300";
                cbDPI.IsChecked = true;
                cbGroup.IsChecked = ValToBool(opts[9]);
                cbOverPrint.IsChecked = opts[6] == "True";
                cbOverPrint.IsChecked = false;
                cbBitmapToPowerClip.IsChecked = opts[7] == "False";
                cbInnerShadow.IsChecked = false;

                _byHand = false;
            }
            catch (Exception err) { MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error); _byHand = false; }
        }

        private double ConvertUnit(string val, string from, string to) { return DApp.ConvertUnits(str2dbl(val), GetUnit(from), GetUnit(to)); }
        private cdrUnit GetUnit(string unit)
        {
            switch (unit)
            {
                case "cm": return cdrUnit.cdrCentimeter;
                case "pt": return cdrUnit.cdrPoint;
                case "px": return cdrUnit.cdrPixel;
                case "inch": return cdrUnit.cdrInch;
                default: return cdrUnit.cdrMillimeter;
            }
        }

        private void DecodeOldColor(string col)
        {
            try
            {
                int cm = Int32.Parse(col.Substring(0, 4), NumberStyles.HexNumber);
                int c1 = Int32.Parse(col.Substring(4, 4), NumberStyles.HexNumber);
                int c2 = Int32.Parse(col.Substring(8, 4), NumberStyles.HexNumber);
                int c3 = Int32.Parse(col.Substring(12, 4), NumberStyles.HexNumber);
                int c4 = Int32.Parse(col.Substring(16, 4), NumberStyles.HexNumber);
                int c5 = Int32.Parse(col.Substring(20, 4), NumberStyles.HexNumber);
                int c6 = Int32.Parse(col.Substring(24, 4), NumberStyles.HexNumber);
                int c7 = Int32.Parse(col.Substring(28, 4), NumberStyles.HexNumber);
                _shColor = DApp.CreateColorEx(cm, c1, c2, c3, c4, c5, c6, c7);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /*
         * The old settings stores in "DS"
         * Color remembers in old way => "138A0000000000000064000000000000"
         * X | Y | Outline | Bloor | Transparen | Mode | Group | To Bitmap | ... | ... | ... | ... | ... | Color
         */

        private void boostStart(string undo)
        {
            DApp.ActiveDocument.BeginCommandGroup(undo);
            DApp.Optimization = true;
            DApp.EventsEnabled = false;
            DApp.ActiveDocument.SaveSettings();
            DApp.ActiveDocument.PreserveSelection = false;
        }
        private void boostFinish()
        {
            DApp.ActiveDocument.PreserveSelection = true;
            DApp.ActiveDocument.ResetSettings();
            DApp.EventsEnabled = true;
            DApp.Optimization = false;
            DApp.ActiveDocument.EndCommandGroup();
            DApp.Refresh();
        }

        /*=============================================================================*/
        /* UI */

        private void cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_byHand)
            {
                var combo = (ComboBox)sender;
                if (combo.SelectedItem.ToString().Length > 0)
                {
                    switch (combo.Name)
                    {
                        case "cbMode": SaveKeyVal("BlendMode", combo.SelectedItem.ToString()); break;
                    }
                }
            }
        }

        private void Slider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var sl = (Slider)sender;
            string val = Math.Round(sl.Value).ToString(CultureInfo.InvariantCulture);
            switch (sl.Name)
            {
                case "sOpacity": SaveKeyVal("Opacity", val); break;
                case "sDistance": SaveKeyVal("Distance", val); break;
                case "sSize": SaveKeyVal("Size", val); break;
                case "sFeather": SaveKeyVal("Feather", val); break;
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_byHand)
            {
                var sl = (Slider)sender;
                string val = Math.Round(e.NewValue).ToString(CultureInfo.InvariantCulture);
                switch (sl.Name)
                {
                    case "sOpacity": tbOpacity.Text = val; break;
                    case "sDistance": tbDistance.Text = val; break;
                    case "sSize": tbSize.Text = val; break;
                    case "sFeather": tbFeather.Text = val; break;
                }
            }
        }

        private void SliderTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            var tb = (TextBox)sender;
            double d = str2dbl(tb.Text);
            if (d < 0) { d = 0; tb.Text = d.ToString(CultureInfo.InvariantCulture); }

            switch (tb.Name)
            {
                case "tbOpacity":
                    if (d > 100) { d = 100; tb.Text = d.ToString(CultureInfo.InvariantCulture); }
                    SaveKeyVal("Opacity", d.ToString(CultureInfo.InvariantCulture));
                    break;
                case "tbDistance":
                    SaveKeyVal("Distance", d.ToString(CultureInfo.InvariantCulture));
                    break;
                case "tbSize":
                    SaveKeyVal("Size", d.ToString(CultureInfo.InvariantCulture));
                    break;
                case "tbFeather":
                    if (d > 250) { d = 250; tb.Text = d.ToString(CultureInfo.InvariantCulture); }
                    SaveKeyVal("Feather", d.ToString(CultureInfo.InvariantCulture));
                    break;
            }
        }

        private void ChangeSlider(object sender, TextChangedEventArgs e)
        {
            _byHand = true;

            var tb = (TextBox)sender;
            double d = str2dbl(tb.Text);
            switch (tb.Name)
            {
                case "tbOpacity": sOpacity.Value = d; break;
                case "tbDistance": sDistance.Value = d; break;
                case "tbSize": sSize.Value = d; break;
                case "tbFeather": sFeather.Value = d; break;
            }

            _byHand = false;
        }

        private Point getPoint(double x, double y, double angle, double distance)
        {
            double theta = (Math.PI / 180) * angle; //ConvertToRadians
            Point p = new Point();
            p.X = x + distance * Math.Cos(theta);
            p.Y = y + distance * Math.Sin(theta);
            return p;
        }

        private void Ang_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { _isMove = true; }
        private void MainWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isMove = false;
            if (_angleNotUpdate)
            {
                SaveKeyVal("Angle", tbAngle.Text);
                _angleNotUpdate = false;
            }
        }
        private void MainWindow_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isMove)
            {
                Point curP = Mouse.GetPosition(this);
                Point rc = ang.TranslatePoint(new Point(ang.ActualHeight / 2, ang.ActualWidth / 2), this);
                double angle = Angle(rc, curP);
                string val = GetDrawAngle(Math.Round(angle)).ToString();
                tbAngle.Text = val;
                ang.RenderTransform = new RotateTransform(Math.Round(angle * -1));
                _angleNotUpdate = true;
            }
        }
        private double Angle(Point start, Point end) { return Math.Atan2(start.Y - end.Y, end.X - start.X) * Rad2Deg; }
        private double GetDrawAngle(double r)
        {
            if (r < 0) return 180 + (180 - (r * -1));
            else return r;
        }
        private void tbAngle_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isMove)
            {
                TextBox tb = (TextBox)sender;
                double r = str2dbl(tb.Text);
                if (r > 180) r = (180 - (r - 180)) * -1;
                ang.RenderTransform = new RotateTransform(Math.Round(r * -1));
            }
        }

        private double Distance2D(Point start, Point end)
        {
            double result = 0;
            double part1 = Math.Pow((end.X - start.X), 2);
            double part2 = Math.Pow((end.Y - start.Y), 2);
            double underRadical = part1 + part2;
            result = Math.Sqrt(underRadical);
            return result;
        }

        /* UI COLOR */
        private void ShadowColor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_shColor.UserAssignEx())
            {
                applyColorForUI();
                SaveKeyVal("Color", _shColor.ToString());
            }
        }

        private void menuConvertColorTo(object sender, RoutedEventArgs e)
        {
            var mi = (MenuItem)sender;
            switch (mi.Name)
            {
                case "toRGB": if (_shColor.Type != cdrColorType.cdrColorRGB) _shColor.ConvertToRGB(); break;
                case "toCMYK": if (_shColor.Type != cdrColorType.cdrColorCMYK) _shColor.ConvertToCMYK(); break;
                case "toGRAY": if (_shColor.Type != cdrColorType.cdrColorGray) _shColor.ConvertToGray(); break;
            }
            applyColorForUI();
            SaveKeyVal("Color", _shColor.ToString());
        }

        private void applyColorForUI()
        {
            Corel.Interop.VGCore.Color nRGB = DApp.CreateRGBColor(0, 0, 0);
            nRGB.CopyAssign(_shColor);

            if (nRGB.Type != cdrColorType.cdrColorRGB) nRGB.ConvertToRGB();
            ShadowColor.Background = new SolidColorBrush(Color.FromRgb((byte)nRGB.RGBRed, (byte)nRGB.RGBGreen, (byte)nRGB.RGBBlue));

            ShadowColor.ToolTip = _shColor.Name + "\n" + _shColor.get_Name(true);

            (this.FindName("toRGB") as MenuItem).IsEnabled = true;
            (this.FindName("toCMYK") as MenuItem).IsEnabled = true;
            (this.FindName("toGRAY") as MenuItem).IsEnabled = true;

            switch (_shColor.Type.ToString())
            {
                case "cdrColorRGB": (this.FindName("toRGB") as MenuItem).IsEnabled = false; break;
                case "cdrColorCMYK": (this.FindName("toCMYK") as MenuItem).IsEnabled = false; break;
                case "cdrColorGray": (this.FindName("toGRAY") as MenuItem).IsEnabled = false; break;
            }
        }

        /* HELP */
        private void Help_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (exHelp.IsExpanded)
                {
                    if (sender.GetType().ToString() == "System.Windows.Controls.Border")
                    {
                        Border b = (Border)sender;
                        tbHelp.Text = getHelp(b.Name);
                    }
                    else
                    {
                        Control c = (Control)sender;
                        tbHelp.Text = getHelp(c.Name);
                    }
                }
            }
            catch (Exception err) { MessageBox.Show(err.ToString(), MName, MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private string getHelp(string key)
        {
            XmlNode d = _xHelp.SelectSingleNode("//" + key);
            if (d != null) return d.InnerText;
            else return "";
        }

        private void Expander_Expanded(object sender, System.Windows.RoutedEventArgs e) { ExpanderEx(sender, 1); }
        private void Expander_Collapsed(object sender, System.Windows.RoutedEventArgs e) { ExpanderEx(sender, 0); }
        private void ExpanderEx(object sender, int state)
        {
            if (!_byHand)
            {
                Expander ex = (Expander)sender;
                SaveKeyVal(ex.Name, state.ToString());
            }
        }

        private void tbDPI_KeyUp(object sender, KeyEventArgs e)
        {
            double d = Math.Round(str2dbl(tbDPI.Text));
            int i = Convert.ToInt32(d);
            if (i < 0) { i = 0; tbDPI.Text = i.ToString(); }
            SaveKeyVal("DPI", i.ToString());
        }

        private void cb_Checked(object sender, RoutedEventArgs e) { cb_CheckEx(sender, 1); }
        private void cb_Unchecked(object sender, RoutedEventArgs e) { cb_CheckEx(sender, 0); }
        private void cb_CheckEx(object sender, int state)
        {
            var cb = (CheckBox)sender;
            if (cb.Name == "cbDPI") tbDPI.IsEnabled = state != 1;
            if (cb.Name == "cbInnerShadow")
            {
                cbGroup.IsEnabled = state != 1;
                cbBitmapToPowerClip.IsEnabled = state != 1;
                cbBitmapToPowerClip.IsChecked = state == 1;
            }

            if (!_byHand)
            {
                switch (cb.Name)
                {
                    case "cbDPI": SaveKeyVal("UseDocumentDPI", state.ToString(CultureInfo.InvariantCulture)); break;
                    case "cbGroup": SaveKeyVal("GroupShadowWithParent", state.ToString(CultureInfo.InvariantCulture)); break;
                    case "cbOverPrint": SaveKeyVal("ShadowOverPrint", state.ToString(CultureInfo.InvariantCulture)); break;
                    case "cbBitmapToPowerClip": SaveKeyVal("BitmapToPowerClip", state.ToString(CultureInfo.InvariantCulture)); break;
                    case "cbInnerShadow": SaveKeyVal("InnerShadow", state.ToString(CultureInfo.InvariantCulture)); break;
                }
            }
        }

        /* save key value to xml file */
        private void SaveKeyVal(string keyName, string keyVal)
        {
            var xDoc = new XmlDocument();
            xDoc.Load(_uPath);
            var n = xDoc.SelectSingleNode(@"/App/Options/" + keyName);
            if (n != null)
            {
                n.InnerText = keyVal;
                xDoc.Save(_uPath);
            }
        }

        private void SaveAllSettings()
        {
            var xDoc = new XmlDocument();
            xDoc.Load(_uPath);
            xDoc.SelectSingleNode(@"/App/Options/BlendMode").InnerText = cbMode.SelectedValue.ToString();
            xDoc.SelectSingleNode(@"/App/Options/Color").InnerText = _shColor.ToString();
            xDoc.SelectSingleNode(@"/App/Options/Opacity").InnerText = tbOpacity.Text;
            xDoc.SelectSingleNode(@"/App/Options/Angle").InnerText = tbAngle.Text;
            xDoc.SelectSingleNode(@"/App/Options/Distance").InnerText = tbDistance.Text;
            xDoc.SelectSingleNode(@"/App/Options/Size").InnerText = tbSize.Text;
            xDoc.SelectSingleNode(@"/App/Options/Feather").InnerText = tbFeather.Text;
            xDoc.SelectSingleNode(@"/App/Options/DPI").InnerText = tbDPI.Text;
            xDoc.SelectSingleNode(@"/App/Options/UseDocumentDPI").InnerText = BoolToVal((bool)cbDPI.IsChecked);
            xDoc.SelectSingleNode(@"/App/Options/GroupShadowWithParent").InnerText = BoolToVal((bool)cbGroup.IsChecked);
            xDoc.SelectSingleNode(@"/App/Options/ShadowOverPrint").InnerText = BoolToVal((bool)cbOverPrint.IsChecked);
            xDoc.SelectSingleNode(@"/App/Options/BitmapToPowerClip").InnerText = BoolToVal((bool)cbBitmapToPowerClip.IsChecked);
            xDoc.SelectSingleNode(@"/App/Options/InnerShadow").InnerText = BoolToVal((bool)cbInnerShadow.IsChecked);
            xDoc.Save(_uPath);
        }

        /* string to double */
        private double str2dbl(string s)
        {
            try
            {
                string decimal_sep = System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
                string wrongSep = decimal_sep == "." ? "," : ".";
                return double.Parse(s.Replace(wrongSep, decimal_sep));
            }
            catch (Exception) { return 0; }
        }

        private void AboutBtn_Click(object sender, RoutedEventArgs e)
        {
            var w = new wAbout();
            var wih = new System.Windows.Interop.WindowInteropHelper(w) { Owner = (IntPtr)DApp.AppWindow.Handle };
            w.ShowDialog();
        }

        private void LangBtn_Click(object sender, RoutedEventArgs e)
        {
            LangMenu.PlacementTarget = this;
            LangMenu.IsOpen = true;
        }

        private void ChangeLang(object sender, RoutedEventArgs e)
        {
            try
            {
                var mi = (MenuItem)sender;
                RegistryKey Key;
                string id = mi.Tag.ToString();

                Key = Registry.CurrentUser.CreateSubKey("Software\\CDRPRO MACROS\\" + Docker.MName);
                if (Key == null) Key = Registry.CurrentUser.CreateSubKey("Software\\CDRPRO MACROS\\" + Docker.MName);
                Key.SetValue("Lang", id, RegistryValueKind.String);

                string uFolderPath = Environment.GetEnvironmentVariable("APPDATA") + @"\Corel\" + MName;
                ULangPath = uFolderPath + @"\" + id + @".xml";
                LoadLang(this, "Lang");

                _xHelp = new XmlDocument();
                _xHelp.Load(ULangPath);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), Docker.MName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class DropShadowPreset
    {
        public string name { get; set; }
        public string value { get; set; }
        public DropShadowPreset(string sName, string sValue)
        {
            this.name = sName;
            this.value = sValue;
        }
    }
}
