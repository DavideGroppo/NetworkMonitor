using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace NetworkMonitor;

public partial class MonitorWindow : Window
{

  public MonitorWindow()
  {
    InitializeComponent();
  }

  protected override void OnDeactivated(EventArgs e) => base.OnDeactivated(e);


}
