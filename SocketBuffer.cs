using UnityEngine;
using System.Collections;

using System;

using System.IO;





public delegate void CallBackRecvOver(byte[] allData);
public class SocketBuffer 
{
    //定义消息头
    private byte[] headByte;

    // 消息头的长度
    private byte headLength  =6;


    // 一个消息的 所有字节  1024 
    private byte[] allRecvData;//接受到的数据


    //当前接受到的数据长度  512
    private int curRecvLength;


    //总共接受的数据长度。
    private int allDataLength;


    public SocketBuffer(byte tmpHeadLength,CallBackRecvOver  tmpOver)
    {
        headLength = tmpHeadLength;
        headByte = new byte[headLength] ;

        callBackRecvOver = tmpOver;



    }

    // 表示从   EndReciv  拿到的数据 buffer     读取的真实长度
    public void RecvByte(byte[] recvByte, int realLenth)
    {
        if (realLenth == 0)
            return;

        //当前接受的数据 小于头的长度。
        if (curRecvLength < headByte.Length)
        {
            // 拼凑成 头部 消息
            RecvHead(recvByte, realLenth);
        }
        else
        {

            //  一情况 从socket 都进来的 分析  
            // 二  ： 剩余的 字节流 
            // 这里面 都是头部已经 全部组装完毕 。

            //接受的总长度  6  +1020
            int tmpLength = curRecvLength + realLenth;

            if (tmpLength == allDataLength)
            {
                 //  刚好相等的情况

                RecvOneAll(recvByte, realLenth);


            }
                // 丢进来的长度  大于 一个消息的长度
            else if (tmpLength > allDataLength)  // 接受的数据比这个消息长
            {
                // 凑成一个 消息体  并且 还有剩余
                RecvLarger(recvByte, realLenth);

            }
            else
            {
                RecvSmall(recvByte, realLenth);
            }


        }

    }


    private void RecvLarger(byte[] recvByte, int realLenght)
    {

        //差多少个消息 组成一个完整的消息
        int tmpLength = allDataLength - curRecvLength;

        Buffer.BlockCopy(recvByte, 0, allRecvData, curRecvLength, tmpLength);

        curRecvLength += tmpLength;

        // 整个消息 已经拼接完成 

        RecvOneMsgOver();


        // 还剩余多少格 字节
        int remainLength = realLenght - tmpLength;


        // 剩下的字节 拷贝进  reaminbytes
        byte[]  reaminByte =  new byte[remainLength] ;

        Buffer.BlockCopy(recvByte, tmpLength, reaminByte, 0, remainLength);

        // 看成 从socket 里面取出来放入 处理
        RecvByte(reaminByte,remainLength);


    }

    //  还不能拼成一个完整的消息 
    private void RecvSmall(byte[] recvByte, int realLenght)
    {
        Buffer.BlockCopy(recvByte, 0, allRecvData, curRecvLength, realLenght);

        curRecvLength += realLenght;
    }

    private void RecvOneAll(byte[] recvByte, int realLenght)
    {

        Buffer.BlockCopy(recvByte, 0, allRecvData, curRecvLength, realLenght);

        curRecvLength += realLenght;

        RecvOneMsgOver();


    }


    private void RecvHead(byte[] recByte, int realLenth)
    {

        //差多少个字节 才能组成一个头
        int tmpReal = headByte.Length - curRecvLength;

        //现在接受的 和已经接受的 总长度是多少。 
        int tmpLength = curRecvLength + realLenth;

        //总长度小于头部
        if (tmpLength < headByte.Length)
        {
            // 表示 还没有组成一个 头部 
            Buffer.BlockCopy(recByte, 0, headByte, curRecvLength, realLenth);

            curRecvLength += realLenth;
        }
        else  //大于等于头
        {//100   1024

            Buffer.BlockCopy(recByte, 0, headByte, curRecvLength, tmpReal);

            curRecvLength += tmpReal;
            //头部已经凑齐

             //  取出四个字节 转换 int
            allDataLength = BitConverter.ToInt32(headByte,0) + headLength ;

            // 一个消息的 全部长度
            allRecvData = new byte[allDataLength]; //body +head 


            //allRecvData  已经包含了 头部了
            Buffer.BlockCopy(headByte, 0, allRecvData, 0, headLength);

            //1024 - 2
            int tmpRemin = realLenth - tmpReal;

            //表示 recByte  是否还有数据
            if (tmpRemin > 0)
            {

                byte[]  tmpByte = new byte[tmpRemin] ;

                //表示将剩下的 字节 送入 tmpByte
                Buffer.BlockCopy(recByte, tmpReal, tmpByte, 0, tmpRemin);

                // 剩余的1022 往 
                RecvByte(tmpByte,tmpRemin);

            }
            else
            {
               //只有消息头的情况

                RecvOneMsgOver();
            }





 
        }



    }



    #region  recv over  back to 




    CallBackRecvOver callBackRecvOver;
    private void RecvOneMsgOver()
    {

        //回调函数 返回给  上层 
        if (callBackRecvOver != null)
        {
            // 这个地方 上层 会做很多处理
            callBackRecvOver(allRecvData);
        }

        curRecvLength = 0; 

        allDataLength = 0;

        allRecvData = null;
    }

    #endregion

}
