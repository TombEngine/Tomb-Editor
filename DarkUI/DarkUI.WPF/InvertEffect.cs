using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace DarkUI.WPF;

public class InvertEffect : ShaderEffect
{
	private static readonly PixelShader _shader = new() { UriSource = new Uri("pack://application:,,,/DarkUI.WPF;component/Resources/Invert.ps") };

	public InvertEffect()
	{
		PixelShader = _shader;
		UpdateShaderValue(InputProperty);
	}

	public Brush Input
	{
		get => (Brush)GetValue(InputProperty);
		set => SetValue(InputProperty, value);
	}

	public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty("Input", typeof(InvertEffect), 0);
}
