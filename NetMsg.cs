using UnityEngine;

using System.Collections;

using System;


//
//  NetMsg#FILEEXTENSION#
//  #PROJECTNAME#
//
//  Created by #SMARTDEVELOPERS# on #CREATIONDATE#.
//
//

public class NetMsg  
{


    private byte[] buffer;


    private byte mainId;

    private byte sendId;


    /// <summary>
    ///  从网络 到  类
    /// </summary>
    /// <param name="arr"></param>
    public NetMsg(byte[] arr)
    {
        //arr  整个消息体    head  + body

        buffer = arr;

        mainId = arr[5] ;

        sendId = arr[6];


      //  AnasyBytes();

    }


    public void AnasysImage()
    {

        
    }

    public void AnasyBytes()
    {

        if (mainId == 0 && sendId == 1)
        {

            //  从 消息头 拿 四个字节 出来  变成 消息的 长度。
            int tmpLength = BitConverter.ToInt32(buffer, 0);


            byte[] tmpBytes = new byte[tmpLength];

            Buffer.BlockCopy(buffer, 6, tmpBytes, 0, tmpLength);

          //  test.TestMsg tmpMsg = ProtoTools.ProtoDeserialize<test.TestMsg>(tmpBytes);

            //
          //  UIManager  上传到  UI界面  M 层   。 进行 逻辑处理 。
        }

    }


    // 从 类 到bytes

    /// <summary>
    ///         test.TestMsg tmpMsg = new test.TestMsg();

    ///byte[] tmpMsgByte = ProtoTools.ProtoSerialize(tmpMsg);
    ///
    /// NetMsg(tmpMsgByte, 0 ,1)
    /// </summary>
    /// <param name="arr"></param>
    /// <param name="main"></param>
    /// <param name="second"></param>

    public NetMsg(byte[] arr,byte main, byte  second)
    {

        //  




        //  arr 要发送的 类的 字节流

        int  tmpLength =  arr.Length +6 ;

        // 分配的整个 消息体
        buffer = new byte[tmpLength];  // body  +  head  字节流


        byte[] lengthBytes = BitConverter.GetBytes(arr.Length);

        Buffer.BlockCopy(lengthBytes, 0, buffer, 0, 4);



        buffer[5] = main;

        buffer[6] = second;


      /// 拷贝 消息 进入  byte[]  

        Buffer.BlockCopy(arr, 0, buffer,6,arr.Length);


  

   


    }




    public byte[] GetNetBytes()
    {
        return buffer;
    }


}
