   M S B A M L   `   `   `  ����L  HSerialPortMonitor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullA%clr-namespace:SerialPortMonitor.ViewsSerialPortMonitor.Views  5      K*clr-namespace:SerialPortMonitor.ViewModelsSerialPortMonitor.ViewModels  5      2    ,SerialPortMonitor.Views.SettingsTabPresenter   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      7local%clr-namespace:SerialPortMonitor.Views     5      8m*clr-namespace:SerialPortMonitor.ViewModels     5      .��5      �   � RowDefinitions  5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6    � ColumnDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   $	��200��6,   .+�5   
   �� $��0��6   $��0��6!   $0��*Порт для прослушивания��5      �� 5   
   $��0��6   $��1��6    $��10,0,0,0q�60   ��5      �� *AvailablePorts+ �� SelectedItem 5      �� *SelectedPort+ �� Mode$ TwoWay=��� 5   
   $��1��6   $��0��6!   $��0,10,0,0q�61   $6��0Количество читаемых чисел��5      �� 5   
   $��1��6   $��1��6   $��	10,10,0,0q�6.    � Minimum$ 1Q�5        � Maximum$ 10Q�6    �� TickFrequency$ 1Q�5!       �� TickPlacement$ BottomRight=�6#    �� Ticks$ 1,2,3,4,5,6,7,8,9,10R�6?   	 �� IsSnapToTickEnabled	 . 
 �� AutoToolTipPlacement$
 TopLeft=�5"   ,    � Value 5    *   �� *CountOfNumbers+$ TwoWay=�5"   ,   5#      5$      