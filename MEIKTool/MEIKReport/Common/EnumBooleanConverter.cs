using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MEIKReport.Common
{
    /// <summary>
    /// 通用的枚举布尔转换器，主要用户RadioButton的绑定处理
    /// 绑定使用方法如下：
    /// 窗体引用：xmlns:cvt="clr-namespace:MEIKReport.Common"
    /// <Grid>
    ///    <Grid.Resources>
    ///        <cvt:EnumBooleanConverter x:Key="enumBooleanConverter" />
    ///    </Grid.Resources>
    ///    <StackPanel >
    ///        <RadioButton IsChecked="{Binding Path=VeryLovelyEnum, Converter={StaticResource enumBooleanConverter}, ConverterParameter=FirstSelection}">first selection</RadioButton>
    ///        <RadioButton IsChecked="{Binding Path=VeryLovelyEnum, Converter={StaticResource enumBooleanConverter}, ConverterParameter=TheOtherSelection}">the other selection</RadioButton>
    ///        <RadioButton IsChecked="{Binding Path=VeryLovelyEnum, Converter={StaticResource enumBooleanConverter}, ConverterParameter=YetAnotherOne}">yet another one</RadioButton>
    ///   </StackPanel>
    ///</Grid>
    /// </summary>
    public class EnumBooleanConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return DependencyProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return DependencyProperty.UnsetValue;

            object parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return DependencyProperty.UnsetValue;

            return Enum.Parse(targetType, parameterString);
        }
        #endregion
    }
}
