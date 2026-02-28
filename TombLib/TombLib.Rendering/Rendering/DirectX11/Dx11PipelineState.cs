using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace TombLib.Rendering.DirectX11
{
    public unsafe class Dx11PipelineState : IDisposable
    {
        private static Assembly ThisAssembly = Assembly.GetExecutingAssembly();
        public ComPtr<ID3D11VertexShader> VertexShader;
        public ComPtr<ID3D11PixelShader> PixelShader;
        public ComPtr<ID3D11InputLayout> InputLayout;

        public Dx11PipelineState(Dx11RenderingDevice device, string shaderName, InputElementDesc[] inputElements)
        {
            // Vertex shader
            using (Stream VertexShaderStream = ThisAssembly.GetManifestResourceStream("DxShaders." + shaderName + "VS"))
            {
                if (VertexShaderStream == null)
                    throw new Exception("Vertex shader for \"" + shaderName + "\" not found.");
                byte[] VertexShaderBytes = new byte[VertexShaderStream.Length];
                VertexShaderStream.Read(VertexShaderBytes, 0, VertexShaderBytes.Length);

                fixed (byte* pBytecode = VertexShaderBytes)
                {
                    ID3D11VertexShader* pVS = null;
                    SilkMarshal.ThrowHResult(
                        device.Device.Handle->CreateVertexShader(pBytecode, (nuint)VertexShaderBytes.Length, (ID3D11ClassLinkage*)null, &pVS));
                    VertexShader = new ComPtr<ID3D11VertexShader>(pVS);
                    Dx11RenderingDeviceDebugging.SetDebugName((ID3D11DeviceChild*)pVS, shaderName);

                    // Input layout
                    fixed (InputElementDesc* pElements = inputElements)
                    {
                        ID3D11InputLayout* pIL = null;
                        SilkMarshal.ThrowHResult(
                            device.Device.Handle->CreateInputLayout(pElements, (uint)inputElements.Length, pBytecode, (nuint)VertexShaderBytes.Length, &pIL));
                        InputLayout = new ComPtr<ID3D11InputLayout>(pIL);
                        Dx11RenderingDeviceDebugging.SetDebugName((ID3D11DeviceChild*)pIL, shaderName);
                    }
                }
            }

            // Pixel shader
            using (Stream PixelShaderStream = ThisAssembly.GetManifestResourceStream("DxShaders." + shaderName + "PS"))
            {
                if (PixelShaderStream == null)
                    throw new Exception("Pixel shader for \"" + shaderName + "\" not found.");
                byte[] PixelShaderBytes = new byte[PixelShaderStream.Length];
                PixelShaderStream.Read(PixelShaderBytes, 0, PixelShaderBytes.Length);

                fixed (byte* pBytecode = PixelShaderBytes)
                {
                    ID3D11PixelShader* pPS = null;
                    SilkMarshal.ThrowHResult(
                        device.Device.Handle->CreatePixelShader(pBytecode, (nuint)PixelShaderBytes.Length, (ID3D11ClassLinkage*)null, &pPS));
                    PixelShader = new ComPtr<ID3D11PixelShader>(pPS);
                    Dx11RenderingDeviceDebugging.SetDebugName((ID3D11DeviceChild*)pPS, shaderName);
                }
            }
        }

        public void Apply(ID3D11DeviceContext* context)
        {
            context->VSSetShader(VertexShader.Handle, (ID3D11ClassInstance**)null, 0);
            context->PSSetShader(PixelShader.Handle, (ID3D11ClassInstance**)null, 0);
            context->IASetInputLayout(InputLayout.Handle);
            context->IASetPrimitiveTopology(Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglelist);
        }

        public void Apply(ID3D11DeviceContext* context, RenderingStateBuffer stateBuffer0)
        {
            Apply(context);
            var dxStateBuffer0 = (Dx11RenderingStateBuffer)stateBuffer0;
            ID3D11Buffer* pCB = dxStateBuffer0.ConstantBuffer.Handle;
            context->PSSetConstantBuffers(0, 1, &pCB);
            context->VSSetConstantBuffers(0, 1, &pCB);
        }

        public void Dispose()
        {
            VertexShader.Dispose();
            PixelShader.Dispose();
            InputLayout.Dispose();
        }
    }
}
