   M S B A M L   `   `   `  ����H  DRotationDraft, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:RotationDraftRotationDraft  5   	       RotationDraft.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -localclr-namespace:RotationDraft     5   	     � Title$  BMP ROTATION APP��5   	   $	��450��6"   $	��625��6/   .��5	      �  � ColumnDefinitions 5
   
   �� 5      $
��307* �6   �� 5      $
��310* �6    � RowDefinitions 5   
   �� 5      $
��343* �6   �� 5      $	��76* �6   .+�5   
   �� -   5         ��Name$ 	sourceImg��5   -   $��0��6   $��0��6   $��test_small.jpg��6@    � ToolTip$- 'Исходное изображение��6X   �� 5   
   -   5        $ 
rotatedImg��5   -   $��0��6   $��1��6   $9 3Результирующее изображение��6A   �� 5   
   -   5         ��Name$ 
loadImgBtn��5      $��Загрузить бмп��6%   $
��Left=�6=   $��32��6X   $��	87,23,0,0q�6d   $��1��6w   $	��Top=�6�   $	��123��6�   $S MЗагрузить исходное растровое изображение��6�   �� 5   
   -   5         ��Name$ angleValueSldr��5      $��1��6)   $
��Left=�6d   $��23��6    � Minimum$
 -180Q�6�    � Maximum$	 180Q�6�   $��	10,10,0,0q�6�   $��1��6�   $	��Top=�6�   $	��279��6�   �� 5   
   -   5        	 ��Name$	 textBox��5      $��1��6#   $
��Left=�63   $��33��6d   $��
111,33,0,0q�6p   $��1��6�   
 �� TextWrapping$

 Wrap=�6�   $��14(�6�   $��Center=�6�   $��Center=�6�   $	��Top=�6P  $��83��6h  $Y SВведите угол поворота в градусах и нажмите ОК��6s  ��6�   ��  �� ElementName$ angleValueSldr�� �� Path$ Value� �� UpdateSourceTrigger$ 	LostFocus=�6s  �� 5   
   -   5         ��Name$ 	textBlock��5      $��1��6'   $
��Left=�67   $��22��6R   $��	10,38,0,0q�6^   $��1��6q   $
��Wrap=�6~   $ ��Угол поворота:��6�   $	��Top=�6�   $��96��6�   � 5   
   -   5         �Name$ progress��5      $��1��6(   $
��Left=�68   $��21��6S   $��
209,39,0,0q�6_   $��1��6s   $	��Top=�6�   $��80��6�   5      5      