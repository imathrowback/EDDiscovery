﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EDDiscovery.DB;
using Newtonsoft.Json.Linq;
using EDDiscovery;
using System.IO;
using System.Diagnostics;
using ExtendedControls;

namespace EDDiscovery2
{
    public class EDDTheme
    {
        public static readonly string[] ButtonStyles = "System Flat Gradient".Split();
        public static readonly string[] TextboxBorderStyles = "None FixedSingle Fixed3D Colour".Split();

        private static string buttonstyle_system = ButtonStyles[0];
        private static string buttonstyle_gradient = ButtonStyles[2];
        private static string textboxborderstyle_fixed3D = TextboxBorderStyles[2];
        private static string textboxborderstyle_color = TextboxBorderStyles[3];


        public struct Settings
        {
            public enum CI
            {
                form,
                button_back, button_border, button_text,
                grid_border, grid_background, grid_bordertext, grid_text, grid_borderlines,
                textbox_fore, textbox_highlight, textbox_success,textbox_back, textbox_border,
                menu_back, menu_fore,
                group_back, group_text, group_borderlines,
                travelgrid_nonvisted, travelgrid_visited,
                checkbox,
                label,
                tabcontrol_borderlines
            };

            public string name;         // name of scheme
            public Dictionary<CI, Color> colors;       // dictionary of colors, indexed by CI.
            public bool windowsframe;
            public double formopacity;
            public string fontname;         // Font.. (empty means don't override)
            public float fontsize;
            public string buttonstyle;
            public string textboxborderstyle;



            public Settings(String n, Color f,
                                        Color bb, Color bf, Color bborder, string bstyle,
                                        Color gb, Color gbt, Color gbck, Color gt, Color gridlines,
                                        Color tn, Color tv, 
                                        Color tbb, Color tbf, Color tbh, Color tbs, Color tbborder, string tbbstyle , 
                                        Color c,
                                        Color mb, Color mf,
                                        Color l,
                                        Color grpb, Color grpt, Color grlines,
                                        Color tabborderlines,
                                        bool wf, double op, string ft, float fs)            // ft = empty means don't set it
            {
                name = n;
                colors = new Dictionary<CI, Color>();
                colors.Add(CI.form, f);
                colors.Add(CI.button_back, bb); colors.Add(CI.button_text, bf); colors.Add(CI.button_border,bborder);
                colors.Add(CI.grid_border, gb); colors.Add(CI.grid_bordertext, gbt);
                colors.Add(CI.grid_background, gbck); colors.Add(CI.grid_text, gt); colors.Add(CI.grid_borderlines, gridlines);
                colors.Add(CI.travelgrid_nonvisted, tn); colors.Add(CI.travelgrid_visited, tv);
                colors.Add(CI.textbox_back, tbb); colors.Add(CI.textbox_fore, tbf);
                colors.Add(CI.textbox_highlight, tbh); colors.Add(CI.textbox_success, tbs);
                colors.Add(CI.textbox_border, tbborder);
                colors.Add(CI.checkbox, c);
                colors.Add(CI.menu_back, mb); colors.Add(CI.menu_fore, mf);
                colors.Add(CI.label, l);
                colors.Add(CI.group_back, grpb); colors.Add(CI.group_text, grpt); colors.Add(CI.group_borderlines, grlines);
                colors.Add(CI.tabcontrol_borderlines, tabborderlines);
                buttonstyle = bstyle; textboxborderstyle = tbbstyle;
                windowsframe = wf; formopacity = op; fontname = ft; fontsize = fs;
            }

            public Settings(JObject jo, string settingsname, Settings defcols)            // From json. defcols is the colours to use if the json does not have it.
            {
                name = settingsname.Replace(".eddtheme", "");
                colors = new Dictionary<CI, Color>();

                foreach (CI ck in Enum.GetValues(typeof(CI)))           // all enums
                {
                    Color d = defcols.colors[ck];
                    colors.Add(ck, JGetColor(jo, ck.ToString(),d));
                }

                windowsframe = GetBool(jo["windowsframe"],defcols.windowsframe);
                formopacity = GetFloat(jo["formopacity"],(float)defcols.formopacity);
                fontname = GetString(jo["fontname"], defcols.fontname);
                fontsize = GetFloat(jo["fontsize"], defcols.fontsize);
                buttonstyle = GetString(jo["buttonstyle"], defcols.buttonstyle);
                textboxborderstyle = GetString(jo["textboxborderstyle"], defcols.textboxborderstyle);
            }

            public Settings(string n)                                               // gets you windows default colours
            {
                name = n; 
                colors = new Dictionary<CI, Color>();
                colors.Add(CI.form, SystemColors.Menu);
                colors.Add(CI.button_back, SystemColors.Control); colors.Add(CI.button_text, SystemColors.ControlText); colors.Add(CI.button_border, SystemColors.Menu);
                colors.Add(CI.grid_border, SystemColors.Menu); colors.Add(CI.grid_bordertext, SystemColors.MenuText);
                colors.Add(CI.grid_background, SystemColors.ControlLightLight); colors.Add(CI.grid_text, SystemColors.MenuText); colors.Add(CI.grid_borderlines, SystemColors.ControlDark);
                colors.Add(CI.travelgrid_nonvisted, Color.Blue); colors.Add(CI.travelgrid_visited, SystemColors.MenuText);
                colors.Add(CI.textbox_back, SystemColors.Menu); colors.Add(CI.textbox_fore, SystemColors.MenuText); colors.Add(CI.textbox_highlight, Color.Red); colors.Add(CI.textbox_success, Color.Green); colors.Add(CI.textbox_border, SystemColors.Menu);
                colors.Add(CI.checkbox, SystemColors.MenuText);
                colors.Add(CI.menu_back, SystemColors.Menu); colors.Add(CI.menu_fore, SystemColors.MenuText);
                colors.Add(CI.label, SystemColors.MenuText);
                colors.Add(CI.group_back, SystemColors.Menu); colors.Add(CI.group_text, SystemColors.MenuText); colors.Add(CI.group_borderlines, SystemColors.ControlDark);
                colors.Add(CI.tabcontrol_borderlines, SystemColors.ControlDark);
                buttonstyle = buttonstyle_system;
                textboxborderstyle = textboxborderstyle_fixed3D;
                windowsframe = true;
                formopacity = 100;
                fontname = defaultfont;
                fontsize = defaultfontsize;
            }
                                                            // copy constructor, takes a real copy, with overrides
            public Settings(Settings other,string newname = null , string newfont = null, float newfontsize = 0)                
            {
                name = (newname!=null) ? newname : other.name;
                fontname = (newfont != null) ? newfont : other.fontname;
                fontsize = (newfontsize != 0) ? newfontsize : other.fontsize;
                windowsframe = other.windowsframe; formopacity = other.formopacity;
                buttonstyle = other.buttonstyle; textboxborderstyle = other.textboxborderstyle;
                colors = new Dictionary<CI, Color>();
                foreach (CI ck in other.colors.Keys)
                {
                    colors.Add(ck, other.colors[ck]);
                }
            }

            static private Color JGetColor(JObject jo, string name , Color defc)
            {
                string colstr = GetString(jo[name],null);

                if (colstr == null)
                    return defc;

                return System.Drawing.ColorTranslator.FromHtml(colstr);
            }

            static private bool GetBool(JToken jToken, bool def)
            {
                if (IsNullOrEmptyT(jToken))
                    return def;
                return jToken.Value<bool>();
            }

            static private float GetFloat(JToken jToken, float def)
            {
                if (IsNullOrEmptyT(jToken))
                    return def;
                return jToken.Value<float>();
            }


            static private int GetInt(JToken jToken, int def)
            {
                if (IsNullOrEmptyT(jToken))
                    return def;
                return jToken.Value<int>();
            }

            static private string GetString(JToken jToken,string def)
            {
                if (IsNullOrEmptyT(jToken))
                    return def;
                return jToken.Value<string>();
            }
            
            static private bool IsNullOrEmptyT(JToken token)
            {
                return (token == null) ||
                       (token.Type == JTokenType.Array && !token.HasValues) ||
                       (token.Type == JTokenType.Object && !token.HasValues) ||
                       (token.Type == JTokenType.String && token.ToString() == String.Empty) ||
                       (token.Type == JTokenType.Null);
            }
            
        }

        public static float minfontsize = 4;
        public static string defaultfont = "Microsoft Sans Serif";
        public static float defaultfontsize = 8.25F;

        public string Name { get { return currentsettings.name; } set { currentsettings.name = value; } }

        public Color TextBackColor { get { return currentsettings.colors[Settings.CI.textbox_back]; } set { SetCustom(); currentsettings.colors[Settings.CI.textbox_back] = value; } }
        public Color TextBlockColor { get { return currentsettings.colors[Settings.CI.textbox_fore]; } set { SetCustom(); currentsettings.colors[Settings.CI.textbox_fore] = value; } }
        public Color TextBlockHighlightColor { get { return currentsettings.colors[Settings.CI.textbox_highlight]; } set { SetCustom(); currentsettings.colors[Settings.CI.textbox_highlight] = value; } }
        public Color TextBlockSuccessColor { get { return currentsettings.colors[Settings.CI.textbox_success]; } set { SetCustom(); currentsettings.colors[Settings.CI.textbox_success] = value; } }
        public string TextBlockBorderStyle { get { return currentsettings.textboxborderstyle; } set { SetCustom(); currentsettings.textboxborderstyle = value; } }
        public Color VisitedSystemColor { get { return currentsettings.colors[Settings.CI.travelgrid_visited]; } set { SetCustom(); currentsettings.colors[Settings.CI.travelgrid_visited] = value; } }
        public Color NonVisitedSystemColor { get { return currentsettings.colors[Settings.CI.travelgrid_nonvisted]; } set { SetCustom(); currentsettings.colors[Settings.CI.travelgrid_nonvisted] = value; } }

        public string ButtonStyle { get { return currentsettings.buttonstyle; } set { SetCustom(); currentsettings.buttonstyle= value; } }

        public bool WindowsFrame { get { return currentsettings.windowsframe; } set { SetCustom(); currentsettings.windowsframe = value; } }
        public double Opacity { get { return currentsettings.formopacity; } set { SetCustom(); currentsettings.formopacity = value; } }
        public string FontName { get { return currentsettings.fontname; } set { SetCustom(); currentsettings.fontname = value; } }
        public float FontSize { get { return currentsettings.fontsize; } set { SetCustom(); currentsettings.fontsize = value; } }

        private Settings currentsettings;           // if name = custom, then its not a standard theme..
        private List<Settings> themelist;
        private SQLiteDBClass db;

        public EDDToolStripRenderer toolstripRenderer;

        public EDDTheme()
        {
            toolstripRenderer = new EDDToolStripRenderer();
            themelist = new List<Settings>();           // theme list in
            currentsettings = new Settings("Windows Default");  // this is our default
        }

        public void RestoreSettings()
        {
            if (db == null)
                db = new SQLiteDBClass();

            if (db.keyExists( "ThemeNameOf"))           // if there.. get the others with a good default in case the db is screwed.
            {
                currentsettings.name = db.GetSettingString("ThemeNameOf", "Custom");
                currentsettings.windowsframe = db.GetSettingBool("ThemeWindowsFrame", true);
                currentsettings.formopacity = db.GetSettingDouble("ThemeFormOpacity", 100);
                currentsettings.fontname = db.GetSettingString("ThemeFont", defaultfont);
                currentsettings.fontsize = (float)db.GetSettingDouble("ThemeFontSize", defaultfontsize);
                currentsettings.buttonstyle = db.GetSettingString("ButtonStyle", buttonstyle_system);
                currentsettings.textboxborderstyle = db.GetSettingString("TextBoxBorderStyle", textboxborderstyle_fixed3D);

                foreach (Settings.CI ck in themelist[0].colors.Keys)         // use themelist to find the key names, as we modify currentsettings as we go and that would cause an exception
                {
                    int d = (ck < Settings.CI.button_text) ? SystemColors.Menu.ToArgb() : SystemColors.MenuText.ToArgb();       // pick a good default
                    Color c = Color.FromArgb(db.GetSettingInt("ThemeColor" + ck.ToString(), d));
                    currentsettings.colors[ck] = c;
                }
            }
        }

        public void SaveSettings(string filename)
        {
            if (db == null)
                db = new SQLiteDBClass();

            db.PutSettingString("ThemeNameOf", currentsettings.name);
            db.PutSettingBool("ThemeWindowsFrame", currentsettings.windowsframe);
            db.PutSettingDouble("ThemeFormOpacity", currentsettings.formopacity);
            db.PutSettingString("ThemeFont", currentsettings.fontname);
            db.PutSettingDouble("ThemeFontSize", currentsettings.fontsize);
            db.PutSettingString("ButtonStyle", currentsettings.buttonstyle);
            db.PutSettingString("TextBoxBorderStyle", currentsettings.textboxborderstyle);
            
            foreach (Settings.CI ck in currentsettings.colors.Keys)
            {
                db.PutSettingInt("ThemeColor" + ck.ToString(), currentsettings.colors[ck].ToArgb());
            }

            if ( filename != null )         
                SaveFile(filename);
        }


        public void LoadThemes()
        {
            themelist.Clear();

            themelist.Add(new Settings("Windows Default"));         // windows default..
            
            themelist.Add(new Settings("Orange Delight", Color.Black,
                Color.FromArgb(255, 48, 48, 48), Color.Orange, Color.DarkOrange, buttonstyle_gradient, // button
                Color.FromArgb(255, 176, 115, 0), Color.Black,  // grid border
                Color.Black, Color.Orange, Color.DarkOrange, // grid
                Color.Orange, Color.White, // travel
                Color.Black, Color.Orange, Color.Red, Color.Green, Color.DarkOrange, textboxborderstyle_color, // text box
                Color.Orange, // checkbox
                Color.Black, Color.Orange,  // menu
                Color.Orange,  // label
                Color.FromArgb(255, 32,32,32), Color.Orange, Color.FromArgb(255, 130, 71, 0), // group
                Color.DarkOrange,
                false, 95, "Microsoft Sans Serif", 8.25F));

            themelist.Add(new Settings("Elite EuroCaps", Color.Black,
                Color.FromArgb(255, 48, 48, 48), Color.Orange, Color.DarkOrange, buttonstyle_gradient, // button
                Color.FromArgb(255, 176, 115, 0), Color.Black,  // grid border
                Color.Black, Color.Orange, Color.DarkOrange, // grid
                Color.Orange, Color.White, // travel
                Color.Black, Color.Orange, Color.Red, Color.Green, Color.DarkOrange, textboxborderstyle_color, // text box
                Color.Orange, // checkbox
                Color.Black, Color.Orange,  // menu
                Color.Orange,  // label
                Color.FromArgb(255, 32, 32, 32), Color.Orange, Color.FromArgb(255, 130, 71, 0), // group
                Color.DarkOrange,
                false, 95, "Euro Caps", 12F));

            Color butback = Color.FromArgb(255,32, 32, 32);
            themelist.Add(new Settings("Elite EuroCaps Less Border", Color.Black,
                Color.FromArgb(255, 64, 64, 64), Color.Orange, Color.FromArgb(255, 96, 96, 96), buttonstyle_gradient, // button
                Color.FromArgb(255, 176, 115, 0), Color.Black,  // grid border
                butback, Color.Orange, Color.DarkOrange, // grid
                Color.Orange, Color.White, // travel
                butback, Color.Orange, Color.Red, Color.Green, Color.FromArgb(255,64,64,64), textboxborderstyle_color, // text box
                Color.Orange, // checkbox
                Color.Black, Color.Orange,  // menu
                Color.Orange,  // label
                Color.Black, Color.Orange, Color.FromArgb(255, 130, 71, 0), // group
                Color.DarkOrange,
                false, 100, "Euro Caps", 12F));

            themelist.Add(new Settings(themelist[themelist.Count - 1], "Elite Verdana", "Verdana", 8F));

            themelist.Add(new Settings("EuroCaps Grey",
                                        SystemColors.Menu,
                                        SystemColors.Control, SystemColors.ControlText, Color.DarkGray, buttonstyle_gradient,// button
                                        SystemColors.Menu, SystemColors.MenuText,  // grid border
                                        SystemColors.ControlLightLight, SystemColors.MenuText,  SystemColors.ControlDark, // grid
                                        Color.Blue, SystemColors.MenuText, // travel
                                        SystemColors.Menu, SystemColors.MenuText, Color.Red, Color.Green, Color.DarkGray, textboxborderstyle_color,// text
                                        SystemColors.MenuText, // checkbox
                                        SystemColors.Menu, SystemColors.MenuText,  // menu
                                        SystemColors.MenuText,  // label
                                        SystemColors.Menu, SystemColors.MenuText, SystemColors.ControlDark, // group
                                        SystemColors.ControlDark,
                                        false, 95, "Euro Caps", 12F));

            themelist.Add(new Settings(themelist[themelist.Count - 1], "Verdana Grey", "Verdana", 8F));

            themelist.Add(new Settings("Blue Wonder", Color.DarkBlue,
                                               Color.Blue, Color.White, Color.White, buttonstyle_gradient,// button
                                               Color.DarkBlue, Color.White,  // grid border
                                               Color.DarkBlue, Color.White, Color.Blue, // grid
                                               Color.White, Color.Cyan, // travel
                                               Color.DarkBlue, Color.White, Color.Red, Color.Green, Color.White, textboxborderstyle_color,// text box
                                               Color.White, // checkbox
                                               Color.DarkBlue, Color.White,  // menu
                                               Color.White,  // label
                                               Color.DarkBlue, Color.White, Color.Blue, // group
                                               Color.Blue,
                                               false, 95, "Microsoft Sans Serif", 8.25F));

            Color baizegreen = Color.FromArgb(255, 13, 68, 13);
            themelist.Add(new Settings("Green Baize", baizegreen,
                                               baizegreen, Color.White, Color.White, buttonstyle_gradient,// button
                                               baizegreen, Color.White,  // grid border
                                               baizegreen, Color.White, Color.LightGreen, // grid
                                               Color.White, Color.FromArgb(255, 78, 190, 27), // travel
                                               baizegreen, Color.White, Color.Red, Color.Green, Color.White, textboxborderstyle_color,// text box
                                               Color.White, // checkbox
                                               baizegreen, Color.White,  // menu
                                               Color.White,  // label
                                               baizegreen, Color.White, Color.LightGreen, // group
                                               Color.LightGreen,
                                               false, 95, "Microsoft Sans Serif", 8.25F));

            string themepath = "";

            try
            {
                themepath = Path.Combine(Tools.GetAppDataDirectory(), "Theme");
                if (!Directory.Exists(themepath))
                {
                    Directory.CreateDirectory(themepath);
                }

            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Unable to create the folder '{themepath}'");
                Trace.WriteLine($"Exception: {ex.Message}");

                return;
            }


            // Search for theme files
            DirectoryInfo dirInfo = new DirectoryInfo(themepath);
            FileInfo[] allFiles = null;

            try
            {
                allFiles = dirInfo.GetFiles("*.eddtheme");
            }
            catch
            {
            }

            if (allFiles == null)
            {
                return;
            }


            foreach (FileInfo fi in allFiles)
            {
                try
                {
                    JObject jo = JObject.Parse(File.ReadAllText(fi.FullName));
                    themelist.Add(new Settings(jo, fi.Name,themelist[0]));
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"LoadThemes Exception : {ex.Message}");
                }
            }
        }

        public JObject Settings2Json()
        {
            JObject jo = new JObject();

            foreach (Settings.CI ck in currentsettings.colors.Keys)
            {
                jo.Add(ck.ToString(), System.Drawing.ColorTranslator.ToHtml(currentsettings.colors[ck]));
            }

            jo.Add("windowsframe", currentsettings.windowsframe);
            jo.Add("formopacity", currentsettings.formopacity);
            jo.Add("fontname", currentsettings.fontname);
            jo.Add("fontsize", currentsettings.fontsize);
            jo.Add("buttonstyle", currentsettings.buttonstyle);
            jo.Add("textboxborderstyle", currentsettings.textboxborderstyle);
            return jo;
        }

        public bool SaveFile(string filename)
        {
            string themepath = "";
            JObject jo = Settings2Json();

            try
            {
                themepath = Path.Combine(Tools.GetAppDataDirectory(), "Theme") ;
                if (!Directory.Exists(themepath))
                {
                    Directory.CreateDirectory(themepath);
                }

            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Unable to create the folder '{themepath}'");
                Trace.WriteLine($"Exception: {ex.Message}");

                return false;
            }

            if (filename == null)
            {
                filename = Path.Combine(themepath, currentsettings.name) + ".eddtheme";
            }   

            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.Write(jo.ToString());
            }

            return true;
        }

        public List<string> GetThemeList()
        {
            List<string> result = new List<string>();

            for (int i = 0; i < themelist.Count; i++)
                result.Add(themelist[i].name);

            return result;
        }

        private int FindThemeIndex(string themename)
        {
            for (int i = 0; i < themelist.Count; i++)
            {
                if (themelist[i].name.Equals(themename))
                    return i;
            }

            return -1;
        }

        public int GetIndexOfCurrentTheme()
        {
            return FindThemeIndex(currentsettings.name);
        }

        public bool IsFontAvailableInTheme(string themename, out string fontwanted)
        {
            int i = FindThemeIndex(themename);
            fontwanted = null;

            if (i != -1)
            {
                fontwanted = themelist[i].fontname;
                using (Font fntnew = new Font(fontwanted, themelist[i].fontsize, FontStyle.Regular))
                {
                    return string.Compare(fntnew.Name, fontwanted, true) == 0;
                }
            }

            return false;
        }

        public bool SetThemeByName(string themename)                           // given a theme name, select it if possible
        {
            int i = FindThemeIndex(themename);
            if (i != -1)
            {
                currentsettings = new Settings(themelist[i]);           // do a copy, not a reference assign..
                return true;
            }
            return false;
        }

        public bool IsCustomTheme()
        { return currentsettings.name.Equals("Custom"); }

        public void SetCustom()
        { currentsettings.name = "Custom"; }                                // set so custom..

        public void ApplyColors(Form form)
        {
            form.Opacity = currentsettings.formopacity / 100;
            form.BackColor = currentsettings.colors[Settings.CI.form];

            if (currentsettings.fontname.Equals("") || currentsettings.fontsize < minfontsize)
            {
                currentsettings.fontname = "Microsoft Sans Serif";          // in case schemes were loaded
                currentsettings.fontsize = 8.25F;
            }

            Font fnt = new Font(currentsettings.fontname, currentsettings.fontsize);

            foreach (Control c in form.Controls)
                UpdateColorControls(form,c, fnt, 0);


            UpdateToolsTripRenderer();

    }

    private void UpdateToolsTripRenderer()
    {
            toolstripRenderer.Background = currentsettings.colors[Settings.CI.menu_back];
            toolstripRenderer.MenuText = currentsettings.colors[Settings.CI.menu_fore];

            // SPecial menu border??
            toolstripRenderer.Border = currentsettings.colors[Settings.CI.grid_border];
            toolstripRenderer.BorderLight = currentsettings.colors[Settings.CI.grid_border];

            toolstripRenderer.Dark = currentsettings.colors[Settings.CI.menu_back];
            //Bitmap bmp = new Bitmap(1, 1);
            toolstripRenderer.ButtonSelectedBorder = currentsettings.colors[Settings.CI.textbox_success]; ;
            toolstripRenderer.ButtonSelectBackLight = currentsettings.colors[Settings.CI.button_text];
            toolstripRenderer.ButtonSelectBackDark = currentsettings.colors[Settings.CI.button_back];


        }



    public void UpdateColorControls(Control parent , Control myControl, Font fnt, int level)
        {
#if DEBUG
            //string pad = "                             ".Substring(0, level);
            //Console.WriteLine(pad + parent.Name.ToString() + ":" + myControl.Name.ToString() + " " + myControl.ToString());
#endif

            if (myControl is MenuStrip || myControl is ToolStrip)
            {
                myControl.BackColor = currentsettings.colors[Settings.CI.menu_back];
                myControl.ForeColor = currentsettings.colors[Settings.CI.menu_fore];
                myControl.Font = fnt;
            }
            else if (myControl is RichTextBoxScroll)
            {
                RichTextBoxScroll MyDgv = (RichTextBoxScroll)myControl;
                MyDgv.BorderColor = Color.Transparent;
                MyDgv.BorderStyle = BorderStyle.None;

                MyDgv.TextBox.ForeColor = currentsettings.colors[Settings.CI.textbox_fore];
                MyDgv.TextBox.BackColor = currentsettings.colors[Settings.CI.textbox_back];

                MyDgv.ScrollBar.FlatStyle = FlatStyle.System;

                if (currentsettings.textboxborderstyle.Equals(TextboxBorderStyles[1]))
                    MyDgv.BorderStyle = BorderStyle.FixedSingle;
                else if (currentsettings.textboxborderstyle.Equals(TextboxBorderStyles[2]))
                    MyDgv.BorderStyle = BorderStyle.Fixed3D;
                else if (currentsettings.textboxborderstyle.Equals(TextboxBorderStyles[3]))
                {
                    Color c1 = currentsettings.colors[Settings.CI.textbox_fore];
                    MyDgv.BorderColor = currentsettings.colors[Settings.CI.textbox_border];
                    MyDgv.ScrollBar.BackColor = currentsettings.colors[Settings.CI.textbox_back];
                    MyDgv.ScrollBar.BorderColor = MyDgv.ScrollBar.ThumbBorderColor = MyDgv.ScrollBar.ArrowBorderColor = currentsettings.colors[Settings.CI.textbox_border];
                    MyDgv.ScrollBar.ArrowButtonColor = MyDgv.ScrollBar.ThumbButtonColor = c1;
                    MyDgv.ScrollBar.MouseOverButtonColor = ButtonExt.Multiply(c1, 1.4F);
                    MyDgv.ScrollBar.MousePressedButtonColor = ButtonExt.Multiply(c1, 1.5F);
                    MyDgv.ScrollBar.ForeColor = ButtonExt.Multiply(c1, 0.25F);
                    MyDgv.ScrollBar.FlatStyle = FlatStyle.Popup;
                }

                if (myControl.Font.Name.Contains("Courier"))                  // okay if we ordered a fixed font, don't override
                {
                    Font fntf = new Font(myControl.Font.Name, currentsettings.fontsize); // make one of the selected size
                    myControl.Font = fntf;
                }
                else
                    myControl.Font = fnt;
            }
            else if (myControl is TextBoxBorder)
            {
                TextBoxBorder MyDgv = (TextBoxBorder)myControl;
                myControl.ForeColor = currentsettings.colors[Settings.CI.textbox_fore];
                myControl.BackColor = currentsettings.colors[Settings.CI.textbox_back];
                MyDgv.BorderColor = Color.Transparent;
                MyDgv.BorderStyle = BorderStyle.None;
                MyDgv.BorderPadding = 2;                                                    // for colour selection, 2 pixels of border padding before border..
                MyDgv.AutoSize = true;

                if (currentsettings.textboxborderstyle.Equals(TextboxBorderStyles[0]))
                    MyDgv.AutoSize = false;                                                 // with no border, the autosize clips the bottom of chars..
                else if (currentsettings.textboxborderstyle.Equals(TextboxBorderStyles[1]))
                    MyDgv.BorderStyle = BorderStyle.FixedSingle;
                else if (currentsettings.textboxborderstyle.Equals(TextboxBorderStyles[2]))
                    MyDgv.BorderStyle = BorderStyle.Fixed3D;
                else if (currentsettings.textboxborderstyle.Equals(TextboxBorderStyles[3]))
                    MyDgv.BorderColor = currentsettings.colors[Settings.CI.textbox_border];

                myControl.Font = fnt;
            }
            else if (myControl is ButtonExt)
            {
                ButtonExt MyDgv = (ButtonExt)myControl;
                MyDgv.ForeColor = currentsettings.colors[Settings.CI.button_text];

                if (currentsettings.buttonstyle.Equals(ButtonStyles[0])) // system
                {
                    MyDgv.FlatStyle = FlatStyle.System;
                    MyDgv.UseVisualStyleBackColor = true;           // this makes it system..
                }
                else
                {
                    MyDgv.BackColor = currentsettings.colors[Settings.CI.button_back];
                    MyDgv.FlatAppearance.BorderColor = currentsettings.colors[Settings.CI.button_border];
                    MyDgv.FlatAppearance.BorderSize = 1;
                    MyDgv.FlatAppearance.MouseOverBackColor = ButtonExt.Multiply(currentsettings.colors[Settings.CI.button_back], 1.1F);
                    MyDgv.FlatAppearance.MouseDownBackColor = ButtonExt.Multiply(currentsettings.colors[Settings.CI.button_back], 1.5F);

                    if (currentsettings.buttonstyle.Equals(ButtonStyles[1])) // flat
                        MyDgv.FlatStyle = FlatStyle.Flat;
                    else
                        MyDgv.FlatStyle = FlatStyle.Popup;
                }

                myControl.Font = fnt;
            }
            else if (myControl is TabControlCustom)
            {
                TabControlCustom MyDgv = (TabControlCustom)myControl;

                if (!currentsettings.buttonstyle.Equals(ButtonStyles[0])) // not system
                {
                    MyDgv.FlatStyle = (currentsettings.buttonstyle.Equals(ButtonStyles[1])) ? FlatStyle.Flat : FlatStyle.Popup;
                    MyDgv.TabControlBorderColor = ButtonExt.Multiply(currentsettings.colors[Settings.CI.tabcontrol_borderlines], 0.6F);
                    MyDgv.TabControlBorderBrightColor = currentsettings.colors[Settings.CI.tabcontrol_borderlines];
                    MyDgv.TabNotSelectedBorderColor = ButtonExt.Multiply(currentsettings.colors[Settings.CI.tabcontrol_borderlines], 0.4F);
                    MyDgv.TabNotSelectedColor = currentsettings.colors[Settings.CI.button_back];
                    MyDgv.TabSelectedColor = ButtonExt.Multiply(currentsettings.colors[Settings.CI.button_back], 1.4F);
                    MyDgv.TabMouseOverColor = ButtonExt.Multiply(currentsettings.colors[Settings.CI.button_back], 1.3F);
                    MyDgv.TextSelectedColor = currentsettings.colors[Settings.CI.button_text];
                    MyDgv.TextNotSelectedColor = ButtonExt.Multiply(currentsettings.colors[Settings.CI.button_text], 0.8F);
                    MyDgv.TabStyle = new ExtendedControls.TabStyleAngled();
                }
                else
                    MyDgv.FlatStyle = FlatStyle.System;

                MyDgv.Font = fnt;
            }
            else if (myControl is ComboBoxCustom)
            {
                ComboBoxCustom MyDgv = (ComboBoxCustom)myControl;
                MyDgv.ForeColor = currentsettings.colors[Settings.CI.button_text];

                if (currentsettings.buttonstyle.Equals(ButtonStyles[0])) // system
                {
                    MyDgv.FlatStyle = FlatStyle.System;
                }
                else
                {
                    MyDgv.BackColor = MyDgv.DropDownBackgroundColor = currentsettings.colors[Settings.CI.button_back];
                    MyDgv.BorderColor = currentsettings.colors[Settings.CI.button_border];
                    MyDgv.MouseOverBackgroundColor = ButtonExt.Multiply(currentsettings.colors[Settings.CI.button_back], 1.1F);

                    if (currentsettings.buttonstyle.Equals(ButtonStyles[1])) // flat
                        MyDgv.FlatStyle = FlatStyle.Flat;
                    else
                        MyDgv.FlatStyle = FlatStyle.Popup;
                }

                myControl.Font = fnt;
                MyDgv.Repaint();            // force a repaint as the individual settings do not by design.
            }
            else if (myControl is NumericUpDown)
            {                                                                   // BACK colour does not work..
                myControl.ForeColor = currentsettings.colors[Settings.CI.textbox_fore];
                myControl.Font = fnt;
            }
            else if (myControl is Panel)
            {
                if (!(myControl.Name.Contains("defaultmapcolor")))                 // theme panels show settings color - don't overwrite
                {
                    myControl.BackColor = currentsettings.colors[Settings.CI.form];
                    myControl.ForeColor = currentsettings.colors[Settings.CI.label];
                }
            }
            else if (myControl is Label)
            {
                myControl.ForeColor = currentsettings.colors[Settings.CI.label];
                myControl.Font = fnt;
            }
            else if (myControl is GroupBoxCustom)
            {
                GroupBoxCustom MyDgv = (GroupBoxCustom)myControl;
                MyDgv.ForeColor = currentsettings.colors[Settings.CI.group_text];
                MyDgv.BackColor = currentsettings.colors[Settings.CI.group_back];
                MyDgv.BorderColor = currentsettings.colors[Settings.CI.group_borderlines];
                MyDgv.FlatStyle = FlatStyle.Flat;           // always in Flat, always apply our border.
                MyDgv.Font = fnt;
            }
            else if (myControl is CheckBoxCustom)
            {
                CheckBoxCustom MyDgv = (CheckBoxCustom)myControl;

                if (currentsettings.buttonstyle.Equals(ButtonStyles[0])) // system
                    MyDgv.FlatStyle = FlatStyle.System;
                else if (currentsettings.buttonstyle.Equals(ButtonStyles[1])) // flat
                    MyDgv.FlatStyle = FlatStyle.Flat;
                else
                    MyDgv.FlatStyle = FlatStyle.Popup;

                MyDgv.BackColor = GroupBoxOverride(parent, currentsettings.colors[Settings.CI.form]);
                MyDgv.ForeColor = currentsettings.colors[Settings.CI.checkbox];
                MyDgv.CheckBoxColor = currentsettings.colors[Settings.CI.checkbox];
                MyDgv.CheckBoxInnerColor = ButtonExt.Multiply(currentsettings.colors[Settings.CI.checkbox], 1.5F);
                MyDgv.CheckColor = ButtonExt.Multiply(MyDgv.BackColor, 0.75F);
                MyDgv.MouseOverColor = ButtonExt.Multiply(currentsettings.colors[Settings.CI.checkbox], 1.4F);
                MyDgv.Font = fnt;
            }
            else if (myControl is RadioButtonCustom)
            {
                RadioButtonCustom MyDgv = (RadioButtonCustom)myControl;

                MyDgv.FlatStyle = FlatStyle.System;
                MyDgv.Font = fnt;

                if (currentsettings.buttonstyle.Equals(ButtonStyles[0])) // system
                    MyDgv.FlatStyle = FlatStyle.System;
                else if (currentsettings.buttonstyle.Equals(ButtonStyles[1])) // flat
                    MyDgv.FlatStyle = FlatStyle.Flat;
                else
                    MyDgv.FlatStyle = FlatStyle.Popup;

                //Console.WriteLine("RB:" + myControl.Name + " Apply style " + currentsettings.buttonstyle);

                MyDgv.BackColor = GroupBoxOverride(parent, currentsettings.colors[Settings.CI.form]);
                MyDgv.ForeColor = currentsettings.colors[Settings.CI.checkbox];
                MyDgv.RadioButtonColor = currentsettings.colors[Settings.CI.checkbox];
                MyDgv.RadioButtonInnerColor = ButtonExt.Multiply(currentsettings.colors[Settings.CI.checkbox], 1.5F);
                MyDgv.SelectedColor = ButtonExt.Multiply(MyDgv.BackColor, 0.75F);
                MyDgv.MouseOverColor = ButtonExt.Multiply(currentsettings.colors[Settings.CI.checkbox], 1.4F);
            }
            else if (myControl is DataGridView)                     // we theme this directly
            {
                DataGridView MyDgv = (DataGridView)myControl;
                MyDgv.EnableHeadersVisualStyles = false;            // without this, the colours for the grid are not applied.

                MyDgv.RowHeadersDefaultCellStyle.BackColor = currentsettings.colors[Settings.CI.grid_border];
                MyDgv.RowHeadersDefaultCellStyle.ForeColor = currentsettings.colors[Settings.CI.grid_bordertext];
                MyDgv.ColumnHeadersDefaultCellStyle.BackColor = currentsettings.colors[Settings.CI.grid_border];
                MyDgv.ColumnHeadersDefaultCellStyle.ForeColor = currentsettings.colors[Settings.CI.grid_bordertext];

                MyDgv.BackgroundColor = GroupBoxOverride(parent, currentsettings.colors[Settings.CI.form]);
                MyDgv.DefaultCellStyle.BackColor = GroupBoxOverride(parent, currentsettings.colors[Settings.CI.grid_background]);
                MyDgv.DefaultCellStyle.ForeColor = currentsettings.colors[Settings.CI.grid_text];
                MyDgv.DefaultCellStyle.SelectionBackColor = MyDgv.DefaultCellStyle.ForeColor;
                MyDgv.DefaultCellStyle.SelectionForeColor = MyDgv.DefaultCellStyle.BackColor;

                MyDgv.GridColor = currentsettings.colors[Settings.CI.grid_borderlines];
                MyDgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
                MyDgv.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

                MyDgv.Font = fnt;           // MyDgv.RowHeadersDefaultCellStyle - can't make it work for love nor money..
                Font fnt2;
                
                if (myControl.Name.Contains("dataGridViewTravel") && fnt.Size > 10F)
                    fnt2 = new Font(currentsettings.fontname, 10F);
                else
                    fnt2 = fnt;

                MyDgv.ColumnHeadersDefaultCellStyle.Font = fnt2;
                MyDgv.Columns[0].DefaultCellStyle.Font = fnt2;

                using (Graphics g = MyDgv.CreateGraphics())
                {
                    SizeF sz = g.MeasureString("99999", fnt);
                    MyDgv.RowHeadersWidth = (int)(sz.Width + 2);        // size it to the text
                }
            }
            else if (myControl is VScrollBarCustom && parent is DataViewScrollerPanel)
            {                   // a VScrollbarCustom inside a dataview scroller panel themed as a scroller panel
                VScrollBarCustom MyDgv = (VScrollBarCustom)myControl;

                if (currentsettings.textboxborderstyle.Equals(TextboxBorderStyles[3]))
                {
                    Color c1 = currentsettings.colors[Settings.CI.textbox_fore];
                    MyDgv.BorderColor = currentsettings.colors[Settings.CI.textbox_border];
                    MyDgv.BackColor = currentsettings.colors[Settings.CI.textbox_back];
                    MyDgv.BorderColor = MyDgv.ThumbBorderColor = MyDgv.ArrowBorderColor = currentsettings.colors[Settings.CI.textbox_border];
                    MyDgv.ArrowButtonColor = MyDgv.ThumbButtonColor = c1;
                    MyDgv.MouseOverButtonColor = ButtonExt.Multiply(c1, 1.4F);
                    MyDgv.MousePressedButtonColor = ButtonExt.Multiply(c1, 1.5F);
                    MyDgv.ForeColor = ButtonExt.Multiply(c1, 0.25F);
                    MyDgv.FlatStyle = FlatStyle.Popup;
                }
                else
                    MyDgv.FlatStyle = FlatStyle.System;
            }
            else
            {
                Type tp = myControl.GetType();
                Console.WriteLine("THEME: Unhandled control " + tp.Name + ":" + myControl.Name + " from " + parent.Name);
            }

            foreach (Control subC in myControl.Controls)
            {
                UpdateColorControls(myControl,subC,fnt,level+1);
            }
        }

        public Color GroupBoxOverride(Control parent, Color d)      // if its a group box behind the control, use the group box back color..
        {
            return (parent is GroupBox) ? currentsettings.colors[Settings.CI.group_back] : d;
        }

        public void UpdatePatch( Panel pn )
        {
            Settings.CI ci = (Settings.CI)(pn.Tag);
            pn.BackColor = currentsettings.colors[ci];
        }

        public bool EditColor(Settings.CI ex)                      
        {
            ColorDialog MyDialog = new ColorDialog();
            MyDialog.AllowFullOpen = true;
            MyDialog.FullOpen = true;
            MyDialog.Color = currentsettings.colors[ex];

            if (MyDialog.ShowDialog() == DialogResult.OK)
            {
                currentsettings.colors[ex] = MyDialog.Color;
                SetCustom();
                return true;
            }
            else
                return false;
        }

    }
}
