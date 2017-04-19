using System;
using System.IO;
using Mapgenix.Canvas;


namespace Mapgenix.Styles
{
    /// <summary>Single item in an IconValueStyle.</summary>
    [Serializable]
    public class IconValueItem
    {
        private string _fieldValue;
        private string _iconPathFilename;
        private GeoImage _iconImage;
        private LabelStyle _textStyle;
        private int _textValueLengthMin;
        private int _textValueLengthMax;

       
        public IconValueItem()
            : this(string.Empty, string.Empty, null)
        {

        }

        public IconValueItem(string fieldValue, string iconPathFilename, LabelStyle textStyle)
        {
            _fieldValue = fieldValue;
            _iconPathFilename = iconPathFilename;
            _textValueLengthMax = Int32.MaxValue;
            _textStyle = textStyle;
        }

       
        public IconValueItem(string fieldValue, GeoImage iconImage, LabelStyle textStyle)
        {
            _fieldValue = fieldValue;
            _iconImage = iconImage;
            _textValueLengthMax = Int32.MaxValue;
            _textStyle = textStyle;
        }

        
        public LabelStyle TextStyle
        {
            get { return _textStyle; }
            set { _textStyle = value; }
        }

       
        public string FieldValue
        {
            get { return _fieldValue; }
            set { _fieldValue = value; }
        }

        public string IconFilePathName
        {
            get { return _iconPathFilename; }
            set { _iconPathFilename = value; }
        }

        public int TextValueLengthMin
        {
            get { return _textValueLengthMin; }
            set { _textValueLengthMin = value; }
        }

        public int TextValueLengthMax
        {
            get { return _textValueLengthMax; }
            set { _textValueLengthMax = value; }
        }

        public GeoImage GetIconImage()
        {
            Validators.CheckIconImageAndIconFilePathAreInvalid(_iconPathFilename, _iconImage);

            if (_iconPathFilename != null)
            {
                MemoryStream imageStream = new MemoryStream();

                FileStream imageFile = null;
                try
                {
                    imageFile = File.OpenRead(_iconPathFilename);
                    byte[] imageBytes = new byte[imageFile.Length];
                    imageFile.Read(imageBytes, 0, imageBytes.Length);
                    imageStream.Write(imageBytes, 0, imageBytes.Length);
                }
                finally
                {
                    if (imageFile != null) { imageFile.Close(); imageFile = null; }
                }

                return new GeoImage(imageStream);
            }

            return _iconImage;
        }
    }
}
