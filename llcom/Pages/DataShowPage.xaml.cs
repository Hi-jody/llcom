using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace llcom.Pages
{
    /// <summary>
    /// DataShowPage.xaml 的交互逻辑
    /// </summary>
    public partial class DataShowPage : Page
    {
        public DataShowPage()
        {
            InitializeComponent();
        }

        ScrollViewer sv;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //使日志富文本区域滚动可控制
            sv = uartDataFlowDocument.Template.FindName("PART_ContentHost", uartDataFlowDocument) as ScrollViewer;
            sv.CanContentScroll = true;
            Tools.Logger.DataShowEvent += addUartLog;
            Tools.Logger.DataClearEvent += (xx,x) =>
            {
                uartDataFlowDocument.Document.Blocks.Clear();
            };
        }

        int maxDataLength = 100000;//最长一包数据长度，因为太长会把工具卡死机
        int maxDataPack = 10000;//最大同时显示数据包数，因为太多会把工具卡死机

        /// <summary>
        /// 添加串口日志数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="send">true为发送，false为接收</param>
        private void addUartLog(object e,Tools.DataShowPara input)
        {
            uartDataFlowDocument.IsSelectionEnabled = false;
            byte[] data = input.data;
            bool send = input.send;

            if(Tools.Global.setting.timeout >= 0)
            {
                Paragraph p = new Paragraph(new Run(""));

                Span text = new Span(new Run(DateTime.Now.ToString("[yyyy/MM/dd HH:mm:ss.ffff]")));
                text.Foreground = Brushes.DarkSlateGray;
                p.Inlines.Add(text);

                if (send)
                    text = new Span(new Run(" ← "));
                else
                    text = new Span(new Run(" → "));
                text.Foreground = Brushes.Black;
                text.FontWeight = FontWeights.Bold;
                p.Inlines.Add(text);

                if (data.Length > maxDataLength)
                    text = new Span(new Run(Tools.Global.Byte2String(data.Skip(0).Take(maxDataLength).ToArray())
                        + "\r\nData too long, check log folder for remaining data."));
                else
                    text = new Span(new Run(Tools.Global.Byte2String(data)));

                if (send)
                    text.Foreground = Brushes.DarkRed;
                else
                    text.Foreground = Brushes.DarkGreen;
                text.FontSize = 15;
                p.Inlines.Add(text);

                if (!Tools.Global.setting.showHex)
                    p.Margin = new Thickness(0, 0, 0, 8);
                uartDataFlowDocument.Document.Blocks.Add(p);

                if (Tools.Global.setting.showHex)
                {
                    if (data.Length > maxDataLength)
                        p = new Paragraph(new Run("HEX:" + Tools.Global.Byte2Hex(data.Skip(0).Take(maxDataLength).ToArray(), " ")
                        + "\r\nData too long, check log folder for remaining data."));
                    else
                        p = new Paragraph(new Run("HEX:" + Tools.Global.Byte2Hex(data, " ")));

                    if (send)
                        p.Foreground = Brushes.IndianRed;
                    else
                        p.Foreground = Brushes.ForestGreen;
                    p.Margin = new Thickness(0, 0, 0, 8);
                    uartDataFlowDocument.Document.Blocks.Add(p);
                }

                //条目过多，自动清空
                if (uartDataFlowDocument.Document.Blocks.Count > maxDataPack)
                {
                    uartDataFlowDocument.Document.Blocks.Clear();
                    addUartLog(null, new Tools.DataShowPara
                    {
                        data = Encoding.Default.GetBytes("Too much packs, please check your log folder for log data."),
                        send = true
                    });
                }
            }
            else//不分包
            {
                if(uartDataFlowDocument.Document.Blocks.LastBlock == null ||
                   uartDataFlowDocument.Document.Blocks.LastBlock.GetType() != typeof(Paragraph))
                    uartDataFlowDocument.Document.Blocks.Add(new Paragraph(new Run("")));

                //待显示的数据
                string s;
                if (Tools.Global.setting.showHex)
                    s = Tools.Global.Byte2Hex(data, " ");
                else
                    s = Tools.Global.Byte2String(data);
                Span text = new Span(new Run(s));
                text.FontSize = 15;
                if (send)
                    text.Foreground = Brushes.DarkRed;
                else
                    text.Foreground = Brushes.DarkGreen;
                (uartDataFlowDocument.Document.Blocks.LastBlock as Paragraph).Inlines.Add(text);

            }


            sv.ScrollToBottom();
            uartDataFlowDocument.IsSelectionEnabled = true;
        }
    }
}
