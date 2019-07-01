# ThetaWifiStreaming
Theta S&V Wifi Streaming Demo with Unity

## Things to Prepare 
1. Theta S or Theta V
2. A PC or Mac that is connected to Wifi. iOS or Android will probably work as well. 

## Usage
###THETA S
1. Turn the THETA S on with Wifi mode enabled.
2. Establish the WiFi connection between your PC/Mac and the THETA. The password is written on the THETA, the 8 characters after XS (not including XS)
3. When you open the project in Unity and start a "ThetaSWifiStreamingDemo" scene, the streaming image will appear on the material of Sphere100

###THETA V AP mode
1. Turn the THETA V on with Wifi AP mode enabled.
2. Establish the WiFi connection between your PC/Mac and the THETA. The password is written on the THETA, the 8 characters after YL (not including YL)
3. When you open the project in Unity and start a "ThetaVWifiApModeStreamingDemo" scene, the streaming image will appear on the material of Sphere100

###THETA V Client mode
1. Turn the THETA V on with Wifi Client mode enabled.
2. Follow the links below to establish the WiFi connection between your WiFi rounter and the THETA. https://support.theta360.com/en/manual/v/content/prepare/prepare_08.html
3. Establish the WiFi connection between the WiFi router and your PC/Mac.
4. Open the project in Unity and select a "ThetaVWifiClientModeStreamingDemo" scene then change "Theta Url", "THETA_ID", and "THETA_PASSWORD" in "Theta V Wifi Client Mode Streaming (Script)" in Sphere100 inspector. You can check the URL connected in client mode in [Settings]-> [Camera Settings]-> [Camera Version] of Theta app. The ID is "THETA"+the 10 characters written on the THETA (ex. YL00123456). The initial password is written on the THETA, the 8 characters after YL (not including YL). You can change the passord in the Theta App.
5. When you start a "ThetaVWifiClientModeStreamingDemo" scene, the streaming image will appear on the material of Sphere100.

## Acknowledgements
- I referenced Noshipu's blog. Thank you.  http://noshipu.hateblo.jp/entry/2016/04/21/183439 
- This project uses LitJson to read Json format. https://lbv.github.io/litjson/ Thank you very much.

## Translation of Noshipu's blog

This article was originally written by [@noshipu](https://twitter.com/noshipu),
CEO of [ViRD, Inc](http://vird.co.jp/).

![](http://theta360.guide/blog/img/2016-10/unity1.png)

![](http://theta360.guide/blog/img/2016-10/unity-middle.png)

![](http://theta360.guide/blog/img/2016-10/unity2.png)



# About the RICOH THETA API

In order to use Wifi live streaming, you must use the `_getLivePreview` API.
[Official Reference](https://developers.theta360.com/en/docs/v2.0/api_reference/commands/camera._get_live_preview.html)

> NOTE from Craig: This was replaced by [getLivePreview](https://developers.theta360.com/en/docs/v2.1/api_reference/commands/camera.get_live_preview.html) in version 2.1 of the API. This blog by Noshipu-san refers to the 2.0 API, which is still supported by
the THETA S. Be aware of the differences in your code.

Unlike the other APIs, `_getLivePreview` is different because the data is in a stream and keeps going. You will not be able to get a WWW class to wait until the request is complete (maybe).

> NOTE from Craig: This is the major problem developers have when working with `getLivePreview`. As the data
> is a stream, you can't want for the data to end before running your next command. For example, it's
> different from downloading and displaying an image or video file because you know when the transfer is
> complete.

# Processing Flow

## Set the POST request to create a HttpWebRequest class

    string url = "Enter HTTP path of THETA here";
    var request = HttpWebRequest.Create (url);
    HttpWebResponse response = null;
    request.Method = "POST";
    request.Timeout = (int) (30 * 10000f); // to ensure  no timeout
    request.ContentType = "application/json; charset = utf-8";

    byte [] postBytes = Encoding.Default.GetBytes ( "Put the JSON data here");
    request.ContentLength = postBytes.Length;

## Generate a class of BinaryReader to get the byte data (you get the bytes one by one)

    // The start of transmission of the post data
    Stream reqStream = request.GetRequestStream ();
    reqStream.Write (postBytes, 0, postBytes.Length) ;
    reqStream.Close ();
    stream = request.GetResponse () .GetResponseStream ();

    BinaryReader reader = new BinaryReader (new BufferedStream (stream), new System.Text.ASCIIEncoding ());

## Get the start and stop bytes of 1 frame of the MotionJPEG and cut out one frame

With the byte, check the partion value of the MotionJPEG.

    ...(http)
    0xFF 0xD8      --|
    [jpeg data]      |--1 frame of MotionJPEG
    0xFF 0xD9      --|
    ...(http)
    0xFF 0xD8      --|
    [jpeg data]      |--1 frame of MotionJPEG
    0xFF 0xD9      --|
    ...(http)

Please refer this answer on StackOverflow to
[How to Parse MJPEG HTTP stream from IP camera?](http://stackoverflow.com/questions/21702477/how-to-parse-mjpeg-http-stream-from-ip-camera)  

The starting 2 bytes are `0xFF, 0xD8`. The end bye is `0xD9`

The code is shown below.

    List<byte> imageBytes = new List<byte> ();
    bool isLoadStart = false; // 画像の頭のバイナリとったかフラグ
    byte oldByte = 0; // 1つ前のByteデータを格納する
    while( true ) {
        byte byteData = reader.ReadByte ();

        if (!isLoadStart) {
            if (oldByte == 0xFF){
                // First binary image
               imageBytes.Add(0xFF);
            }
            if (byteData == 0xD8){
               // Second binary image
               imageBytes.Add(0xD8);

               // I took the head of the image up to the end binary
               isLoadStart = true;
            }
        } else {
            // Put the image binaries into an array
            imageBytes.Add(byteData);

            // if the byte was the end byte
            // 0xFF -> 0xD9 case、end byte
            if(oldByte == 0xFF && byteData == 0xD9){
                // As this is the end byte
                // we'll generate the image from the data and can create the texture
                // imageBytes are used to reflect the texture
                // imageBytes are left empty
                // the loop returns the binary image head
                isLoadStart = false;
            }
        }
        oldByte = byteData;
    }

## Texture Generation Separated by Byte

This is the byte to reflect the texture.

    mainTexture.LoadImage ((byte[])imageBytes.ToArray ());

# ThetaWifiStreaming
Theta S Wifi Streaming Demo with Unity

## 準備するもの
1. Theta S
2. Wifiが使えるPC/Mac（iOS/Androidでも多分使えます）

## 使い方
1. Theta S を起動させて、Wifiモードをオンにする。
2. PC/Mac と Theta をWifiで繋げる。パスワードはThetaの底に書いてある、「XS」に後に続く数字８桁（XSは除く）。
3. このプロジェクトをUnityで開いてスタートさせると、Sphere100のマテリアルにストリーミング画像が出る。

## 謝辞
- のしぷさんのブログを参考にさせていただきました。 http://noshipu.hateblo.jp/entry/2016/04/21/183439 ありがとうございました！
- This project uses LitJson to read Json format. https://lbv.github.io/litjson/ Thank you very much.

![Analytics](https://ga-beacon.appspot.com/UA-73311422-5/ThetaWiFiStreaming)
