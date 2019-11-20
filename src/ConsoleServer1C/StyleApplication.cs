using MaterialDesignColors;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ConsoleServer1C
{
    internal class StyleApplication
    {
        private readonly MaterialDesignThemes.Wpf.PaletteHelper _paletteHelper = new MaterialDesignThemes.Wpf.PaletteHelper();
        private readonly IEnumerable<Swatch> _swatches = new SwatchesProvider().Swatches;
        private readonly Swatch _defaultSwatch;

        public StyleApplication()
        {
            _defaultSwatch = _paletteHelper.QueryPalette().PrimarySwatch;
        }

        internal void InitializeContentSelectorColor(Button ButtonChangeColor)
        {
            foreach (Swatch itemSwatches in _swatches)
            {
                MenuItem itemColor = new MenuItem()
                {
                    Background = new SolidColorBrush(itemSwatches.ExemplarHue.Color),
                    Foreground = new SolidColorBrush(itemSwatches.ExemplarHue.Foreground),
                    Header = itemSwatches.Name,
                    Tag = itemSwatches
                };
                itemColor.Click += MenuItemListColorsTheme_Click;
                itemColor.MouseEnter += ItemColor_MouseEnter;
                itemColor.MouseLeave += ItemColor_MouseLeave;

                ButtonChangeColor.ContextMenu.Items.Add(itemColor);
            }
        }

        private void ItemColor_MouseLeave(object sender, MouseEventArgs e)
        {
            ChangeStyleApplication(_defaultSwatch);
        }

        private void ItemColor_MouseEnter(object sender, MouseEventArgs e)
        {
            ChangeStyleApplication(sender);
        }

        private void MenuItemListColorsTheme_Click(object sender, RoutedEventArgs e)
        {
            ChangeStyleApplication(sender);
        }

        internal void ChangeStyleApplication(object sender)
        {
            Swatch tag;
            if (sender is MenuItem senderMenuItem)
                tag = (Swatch)senderMenuItem.Tag;
            else
                tag = (Swatch)sender;

            _paletteHelper.ReplacePrimaryColor(tag);
        }
    }
}
