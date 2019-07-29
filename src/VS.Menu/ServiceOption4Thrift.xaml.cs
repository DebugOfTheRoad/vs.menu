﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VS.Menu.Helper;
using VS.Menu.Model;
using VS.Menu.ThriftGenCore;

namespace VS.Menu
{
    /// <summary>
    /// ServiceOption4Thrift.xaml 的交互逻辑
    /// </summary>
    public partial class ServiceOption4Thrift : Window
    {
        private EnvDTE.DTE _dte;
        private EnumGenType _genType;
        public ServiceOption4Thrift(EnvDTE.DTE dte, EnumGenType genType = EnumGenType.AsyncClientDll)
        {
            _dte = dte;
            _genType = genType;

            InitializeComponent();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region 判断依赖是否保存
            // Nuget包的生成才需要
            lbDependencies.Visibility = Visibility.Hidden;
            if (_genType == EnumGenType.AsyncClientNuget)
            {
                var type = "thrift_" + ThriftGlobal.GenAsyncVersion.ToString();
                var dependencies = DependenceHelper.Get(type);
                if (dependencies == null || dependencies.Count <= 0)
                {
                    ShowDependenceDialog();
                    return;
                }
                lbDependencies.Content = $"共有{dependencies.Count}个依赖";
                lbDependencies.Visibility = Visibility.Visible;
            }
            #endregion

            #region 赋值
            var serviceModel = LocalDataHelper.Get(_dte.ActiveDocument.FullName);

            var configServiceName = serviceModel.ConfigServiceName ?? "";
            if (serviceModel.ServiceName == configServiceName)
                configServiceName = "";

            tbServiceName.Text = serviceModel.ServiceName ?? "";
            tbConfigServiceName.Text = configServiceName;
            tbPort.Text = serviceModel.Port <= 0 ? "" : serviceModel.Port.ToString();
            tbNugetId.Text = serviceModel.NugetId ?? "";
            #endregion
        }

        #region 只能输入数字
        private void tbPort_KeyDown(object sender, KeyEventArgs e)
        {
            #region Tab键直接执行
            if (e.Key == Key.Tab)
            {
                e.Handled = false;
                return;
            }
            #endregion

            var txt = sender as TextBox;
            //屏蔽非法按键
            if ((e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) || e.Key == Key.Decimal)
            {
                if (txt.Text.Contains(".") && e.Key == Key.Decimal)
                {
                    e.Handled = true;
                    return;
                }
                e.Handled = false;
            }
            else if (((e.Key >= Key.D0 && e.Key <= Key.D9) || e.Key == Key.OemPeriod) && e.KeyboardDevice.Modifiers != ModifierKeys.Shift)
            {
                if (txt.Text.Contains(".") && e.Key == Key.OemPeriod)
                {
                    e.Handled = true;
                    return;
                }
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void tbPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            TextChange[] change = new TextChange[e.Changes.Count];
            e.Changes.CopyTo(change, 0);
            int offset = change[0].Offset;
            if (change[0].AddedLength > 0)
            {
                double num = 0;
                if (!Double.TryParse(textBox.Text, out num))
                {
                    textBox.Text = textBox.Text.Remove(offset, change[0].AddedLength);
                    textBox.Select(offset, 0);
                }
            }
        }
        #endregion

        private void btnSure_Click(object sender, RoutedEventArgs e)
        {
            var serviceName = tbServiceName.Text.Trim();
            var configServcieName = tbConfigServiceName.Text.Trim();
            int.TryParse(tbPort.Text.Trim(), out int port);
            var publish = cbPublish.IsChecked;
            var nugetId = tbNugetId.Text.Trim();
            if (string.IsNullOrEmpty(serviceName))
            {
                MessageBox.Show("请输入服务名称\r\n禁止除数字/大小写字母以外的字符", "VSMenu提示");
                return;
            }
            if (!Regex.IsMatch(serviceName, "^[A-Za-z0-9]+$"))
            {
                MessageBox.Show("服务名称为静态类属性名称\r\n禁止除数字/大小写字母以外的字符\r\n\r\nThriftProxy.[服务名称]", "VSMenu提示");
                return;
            }

            if (port <= 0)
            {
                MessageBox.Show("请输入合理的端口号", "VSMenu提示");
                return;
            }

            if (!serviceName.EndsWith("Service"))
            {
                if (serviceName.EndsWith("service"))
                    serviceName = serviceName.TrimEnd("service".ToArray());
                serviceName += "Service";
            }

            if (string.IsNullOrEmpty(configServcieName))
                configServcieName = serviceName;

            var model = new ServiceModel()
            {
                ServiceName = serviceName,
                ConfigServiceName = configServcieName,
                Port = port,
                Publish = publish ?? false,
                NugetId = nugetId,
            };

            #region 记录服务名称跟端口号
            LocalDataHelper.Save(model, _dte.ActiveDocument.FullName);
            #endregion

            var errorMsg = string.Empty;
            var result = ThriftGenerator.Default.GenCsharp(_dte.ActiveDocument.FullName, _genType, out errorMsg, model);
            if (!result)
                MessageBox.Show(errorMsg, "VSMenu提示");

            this.Close();
        }

        private void LbDependencies_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ShowDependenceDialog();
        }

        private void LbSetNugetServer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ShowNugetServerDialog();
        }


        // 自定义方法
        private void ShowDependenceDialog()
        {
            var type = "thrift_" + ThriftGlobal.GenAsyncVersion.ToString();
            var window = new ServiceDependence(type)
            {
                Owner = this
            };
            window.ShowDialog();
        }
        private void ShowNugetServerDialog()
        {
            var window = new ServiceNugetServer()
            {
                Owner = this
            };
            window.ShowDialog();
        }
    }
}