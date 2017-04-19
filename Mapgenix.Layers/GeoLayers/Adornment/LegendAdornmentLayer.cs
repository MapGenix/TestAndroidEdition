using System;
using System.Collections.ObjectModel;
using Mapgenix.Canvas;
using Mapgenix.Utils;
using Mapgenix.Styles;

namespace Mapgenix.Layers
{

    [Serializable]
    public class LegendAdornmentLayer : BaseAdornmentLayer
    {
        SafeCollection<LegendItem> _legendItems = new SafeCollection<LegendItem>();

        float _titleHeight;
        float _footerHeight;

        float _longestWidthInARow;

        Collection<Offset> _offsets;

        LegendItem _title;
        LegendItem _footer;

        

        public LegendItem Title
        {
            get { return _title; }
            set { _title = value; }
        }
        public LegendItem Footer
        {
            get { return _footer; }
            set { _footer = value; }
        }

        public SafeCollection<LegendItem> LegendItems
        {
            get { return _legendItems; }
        }

        protected override void DrawCore(BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {
            AdornmentDrawHelper.DrawLegendAdornmentLayer(this,canvas,labelsInAllLayers);
           
            if (BackgroundMask != null)
            {
                AdornmentDrawHelper.DrawAdornmentCore(this, canvas);
            }
        }

        public void DrawEachLegendItem(Collection<SimpleCandidate> labelsInAllLayers, GdiPlusGeoCanvas adornmentGeoCanvas, int countPostion, LegendItem legendItem)
        {
            if (!IsEnoughPlaceToDraw(legendItem, "LegendItem"))
            {
                return;
            }

            legendItem.Draw(adornmentGeoCanvas, labelsInAllLayers, _offsets[countPostion]);
        }

        public void DrawTitle(Collection<SimpleCandidate> labelsInAllLayers, GdiPlusGeoCanvas adornmentGeoCanvas)
        {
            if (!IsEnoughPlaceToDraw(Title, "Title"))
            {
                return;
            }

            Offset offsets = new Offset();
            offsets.X = 0;
            offsets.Y = 0;

            Title.Draw(adornmentGeoCanvas, labelsInAllLayers, offsets);
        }

        public void DrawFooter(Collection<SimpleCandidate> labelsInAllLayers, GdiPlusGeoCanvas adornmentGeoCanvas)
        {
            if (!IsEnoughPlaceToDraw(Footer, "Footer"))
            {
                return;
            }

            Offset offsets = new Offset();
            offsets.X = 0;
            offsets.Y = adornmentGeoCanvas.Height - Footer.BottomPadding - _footerHeight - Footer.TopPadding;

            Footer.Draw(adornmentGeoCanvas, labelsInAllLayers, offsets);
        }

        public void CountTheOffsetOfEachLegendItem()
        {
            _offsets = new Collection<Offset>();
            Offset currentOffsets = new Offset();
            currentOffsets.X = 0;
            currentOffsets.Y = 0;

            float titlePadding = 0;
            float footerPadding = 0;
            if (Title != null)
            {
                _titleHeight = Title.Height;
                titlePadding = Title.TopPadding + Title.BottomPadding;
            }
            if (Footer != null)
            {
                _footerHeight = Footer.Height;
                footerPadding = Footer.TopPadding + Footer.BottomPadding;
            }

            bool firstLegendItem = true;
            float allItemsHeight = _titleHeight + _footerHeight + titlePadding + footerPadding;
            float lastLegendItemHeight = 0;
            float lastLegendItemTopPadding = 0;
            foreach (LegendItem legendItem in LegendItems)
            {
                Offset tempOffsets = new Offset();
                if (allItemsHeight + legendItem.Height + legendItem.TopPadding + legendItem.BottomPadding <= Height)
                {
                    if (!firstLegendItem)
                    {
                        currentOffsets.Y = currentOffsets.Y + lastLegendItemHeight + lastLegendItemTopPadding;
                    }
                    else
                    {
                        currentOffsets.Y = 0 + _titleHeight + titlePadding;
                    }
                    allItemsHeight = allItemsHeight + legendItem.Height + legendItem.TopPadding;
                    if (legendItem.Width + legendItem.LeftPadding > _longestWidthInARow)
                    {
                        _longestWidthInARow = legendItem.Width + legendItem.LeftPadding;
                    }
                }
                else
                {
                    currentOffsets.X = currentOffsets.X + _longestWidthInARow + legendItem.LeftPadding;
                    currentOffsets.Y = 0 + _titleHeight + titlePadding;
                    allItemsHeight = legendItem.Height + legendItem.TopPadding + _titleHeight + _footerHeight + titlePadding + footerPadding;
                    _longestWidthInARow = legendItem.Width + legendItem.LeftPadding;
                }

                tempOffsets.X = currentOffsets.X;
                tempOffsets.Y = currentOffsets.Y;

                _offsets.Add(tempOffsets);

                firstLegendItem = false;
                lastLegendItemHeight = legendItem.Height + legendItem.BottomPadding;
                lastLegendItemTopPadding = legendItem.TopPadding;
            }
        }

        bool IsEnoughPlaceToDraw(LegendItem legendItem, string type)
        {
            bool result = false;
            double extraHeight = 0;

            if (type.Equals("Footer"))
            {
                extraHeight = _footerHeight + Footer.TopPadding + Footer.BottomPadding;
            }
            else if (type.Equals("LegendItem"))
            {
                double footerTotalHeight = 0;
                if (Footer != null)
                {
                    footerTotalHeight = _footerHeight + Footer.TopPadding + Footer.BottomPadding;
                }
                extraHeight = footerTotalHeight + legendItem.Height + legendItem.TopPadding + legendItem.BottomPadding;
            }
            double titleTotalHeight = 0;
            if (Title != null)
            {
                titleTotalHeight = _titleHeight + Title.TopPadding + Title.BottomPadding;
            }
            if (titleTotalHeight + extraHeight < Height)
            {
                result = true;
            }
            return result;
        }
    }
}
