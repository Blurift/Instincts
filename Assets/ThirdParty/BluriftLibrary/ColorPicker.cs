using UnityEngine;
using System.Collections;

namespace Blurift.UI
{

    public class ColorPicker
    {

        private static int idStart = 1;
        private static int idCounter = 1;

        private float r = 1;
        private float g = 1;
        private float b = 1;
        private float a = 1;
        private float min = 0;
        private float max = 1;

        private bool alpha = false;

        private Rect bounds;
        private Rect rRect;
        private Rect gRect;
        private Rect bRect;
        private Rect aRect;
        private Rect rLabel;
        private Rect gLabel;
        private Rect bLabel;
        private Rect aLabel;
        private Rect titleLabel;

        private string title = "";

        private GUIStyle sliderBar;
        private GUIStyle sliderBarThumb;

        #region Constructors

        public ColorPicker(Rect bounds, bool alpha, string title)
        {
            this.bounds = bounds;
            this.alpha = alpha;
            this.title = title;

            ResetSliders();
        }

        public ColorPicker(Rect bounds, bool alpha) : this(bounds, alpha, "") {}

        public ColorPicker(Rect bounds) : this(bounds, false) { }

        public ColorPicker(float x, float y, float width, float height, bool alpha, string title) : this(new Rect(x, y, width, height), alpha, title) { }

        public ColorPicker(float x, float y, float width, float height, bool alpha) : this(new Rect(x, y, width, height), alpha) { }

        public ColorPicker(float x, float y, float width, float height, string title) : this(new Rect(x, y, width, height), false, title) { }

        public ColorPicker(float x, float y, float width, float height) : this(new Rect(x, y, width, height)) { }

        #endregion

        private void ResetSliders()
        {
            float hMult = 6;
            if (alpha)
                hMult += 2;
            if (title != "")
                hMult += 2;

            float sliderHeight = bounds.height / hMult;

            float sliderLabelW = sliderHeight * 2;
            float sliderLabelH = sliderHeight * 2;

            float sliderWidth = bounds.width - sliderLabelW;

            float sliderY = bounds.y;
            if(title != "")
            {
                titleLabel = new Rect(bounds.x, sliderY, bounds.width, sliderHeight * 2);
                sliderY += sliderHeight * 2;
            }
            rLabel = new Rect(bounds.x, sliderY, sliderLabelW, sliderLabelH);
            rRect = new Rect(bounds.x + sliderLabelW, sliderY + sliderHeight / 2, sliderWidth, sliderHeight);
            sliderY += sliderHeight * 2;
            gLabel = new Rect(bounds.x, sliderY, sliderLabelW, sliderLabelH);
            gRect = new Rect(bounds.x + sliderLabelW, sliderY + sliderHeight / 2, sliderWidth, sliderHeight);
            sliderY += sliderHeight * 2;
            bLabel = new Rect(bounds.x, sliderY, sliderLabelW, sliderLabelH);
            bRect = new Rect(bounds.x + sliderLabelW, sliderY + sliderHeight / 2, sliderWidth, sliderHeight);
            sliderY += sliderHeight * 2;
            aLabel = new Rect(bounds.x, sliderY, sliderLabelW, sliderLabelH);
            aRect = new Rect(bounds.x + sliderLabelW, sliderY + sliderHeight / 2, sliderWidth, sliderHeight);
        }


        public void Set(Color color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }

        public void Set(Vector4 color)
        {
            Set(new Color(color.x, color.y, color.z, color.w));
        }

        public void Set(Vector3 color)
        {
            Set(new Vector4(color.x, color.y, color.z, 1));
        }

        public Color GetColor()
        {
            return new Color(r, g, b, a);
        }

        public Vector3 GetVector3()
        {
            return new Vector3(r, g, b);
        }

        public Vector4 GetVector4()
        {
            return new Vector4(r, g, b, a);
        }

        public void Draw()
        {
            if(sliderBar == null)
            {
                sliderBar = new GUIStyle(GUI.skin.horizontalSlider);
                sliderBar.fixedHeight = rRect.height;
                sliderBarThumb = new GUIStyle(GUI.skin.horizontalSliderThumb);
                sliderBarThumb.fixedHeight = rRect.height * 1.2f;
                sliderBarThumb.stretchHeight = false;
            }

            float sliderY = bounds.y;

            //Old values
            float or = r, og = g, ob = b;


            GUIStyle label = Blurift.BluStyle.CustomStyle(GUI.skin.label, titleLabel.height*0.8f);
            label.alignment = TextAnchor.MiddleCenter;

            GUI.Label(rLabel, "R", label);
            GUI.Label(gLabel, "G", label);
            GUI.Label(bLabel, "B", label);
            r = GUI.Slider(rRect, r, 0.05f, 0, 1, sliderBar, sliderBarThumb, true, idCounter);
            idCounter++;
            g = GUI.Slider(gRect, g, 0.05f, 0, 1, sliderBar, sliderBarThumb, true, idCounter);
            idCounter++;
            b = GUI.Slider(bRect, b, 0.05f, 0, 1, sliderBar, sliderBarThumb, true, idCounter);
            idCounter++;

            if(alpha)
            {
                GUI.Label(aLabel, "A",label);
                a = GUI.Slider(aRect, a, 0.05f, 0, 1, sliderBar, sliderBarThumb, true, idCounter);
                idCounter++;
            }

            if(title != "")
            {
                GUI.Label(titleLabel, title, label);
            }

            float avg = (r + g + b) / 3;

            if(avg < min || avg > max)
            {
                r = or;
                g = og;
                b = ob;
            }
            
        }

        public void SetThreshold(float min,float max)
        {
            this.min = min;
            this.max = max;
        }

        public static void Reset()
        {
            idCounter = idStart;
        }
    }
}
