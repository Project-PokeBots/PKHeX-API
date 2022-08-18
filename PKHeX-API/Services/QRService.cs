using System;
using System.Threading.Tasks;
using PKHeX.Core;
using QRCoder;

namespace PKHeX.API.Services
{

	public class QRService
	{

		public Task<String> GetQRFromFormFile(PKM pkm)
		{
			var QRImage = QRMessageUtil.GetMessage(pkm);

			QRCodeGenerator qRCodeGenerator = new QRCodeGenerator();
			QRCodeData qRCodeData = qRCodeGenerator.CreateQrCode(QRImage, QRCodeGenerator.ECCLevel.Q);
			PngByteQRCode pngByteQRCode = new PngByteQRCode(qRCodeData);
			byte[] qrCodeBytes = pngByteQRCode.GetGraphic(20);
			string qrCodeString = Convert.ToBase64String(qrCodeBytes);

			return Task.FromResult(qrCodeString);
		}
	}
}