﻿using VegaCityApp.API.Constants;

namespace VegaCityApp.API.Utils
{
    public class EnCodeBase64
    {
        public static string EncodeBase64User(string phone, string brandCode)
        {
            // mã hoá QRCode bằng userId và ngày hiện tại
            var currentTime = TimeUtils.GetCurrentSEATime();
            //var currentTimeStamp = TimeUtils.GetTimestamp(currentTime);
            var qrCode = brandCode + "_" + phone + "_" + currentTime;
            //Encode to Base64
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(qrCode);
            var base64 = Convert.ToBase64String(plainTextBytes);
            return base64;
        }
        public static string EncodeBase64Etag(string etagTypeCode)
        {
            var currentTime = TimeUtils.GetCurrentSEATime();
            var qrCode = etagTypeCode + "_" + currentTime + "_" + currentTime.Millisecond;
            //Encode to Base64
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(qrCode);
            var base64 = Convert.ToBase64String(plainTextBytes);
            return base64;
        }
        public static string EncodeBase64<T>(T data)
        {
            var currentTime = TimeUtils.GetCurrentSEATime();
            var qrCode = data + "_" + currentTime + "_" + currentTime.Millisecond;
            //Encode to Base64
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(qrCode);
            var base64 = Convert.ToBase64String(plainTextBytes);
            return base64;
        }

        //public static DecodeBase64Response DecodeBase64Response(string base64)
        //{
        //    //Decode from Base64
        //    var base64EncodedBytes = Convert.FromBase64String(base64);
        //    var qrCode = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        //    var qrCodeSplit = qrCode.Split("_");
        //    var brandCode = qrCodeSplit[0];
        //    var phone = qrCodeSplit[1];
        //    var currentTime = DateTime.Parse(qrCodeSplit[2]);
        //    var response = new DecodeBase64Response
        //    {
        //        BrandCode = brandCode,
        //        Phone = phone,
        //        CurrentTime = currentTime
        //    };
        //    return response;
        //}
    }
}