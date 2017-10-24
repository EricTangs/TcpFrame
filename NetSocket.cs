using UnityEngine;
using System.Collections;


using System.Net;

using System.Net.Sockets;

using System;

using System.Threading;

using System.Collections.Generic;

using System.Text;



public class NetSocket  {


    public delegate void CallBackNormal(bool sucess, ErrorSocket error, string exception);

    public delegate void CallBackRecv(bool sucess, ErrorSocket error, string exception,byte[] byteMessage,string strMessag);



    private CallBackNormal callBackConnect;

    private CallBackNormal callBackSend;

    private CallBackNormal callBackDisconnect;

    private CallBackRecv callBackRecv;


    public enum ErrorSocket
    {

        Sucess =0,
        TimeOut,
        SocketNull,
        SocketUnConnect ,

        ConnnectSucess,
        ConnectUnSucessUnKnow,
        ConnectError,

        SendSucess,
        SendUnSucessUnKown,
        RecvUnSucessUnKown,

        DisConnectSucess,
        DisConnectUnkown,

 
    }

    private ErrorSocket errorSocket;


    private Socket clientSocket;

    private string addressIp;

    private ushort port;

    SocketBuffer recvBuffer;

    public NetSocket()
    {
        recvBuffer = new SocketBuffer(6, RecvMsgOver);


        recvBuf = new byte[1024] ;
    }








    #region  Connect


    public bool IsConnect()
    {
        if (clientSocket.Connected)
            return true;
        return false;
    }

    public void AsyncConnnect(string ip, ushort port, CallBackNormal connectBack, CallBackNormal sendBack, CallBackRecv recvBack, CallBackNormal disBack)
    {
        errorSocket = ErrorSocket.Sucess;

        this.callBackConnect = connectBack;

        this.callBackRecv = recvBack;

        this.callBackSend = sendBack;

        this.callBackDisconnect = disBack;




        if (clientSocket != null && clientSocket.Connected)
        {

            this.callBackConnect(false,ErrorSocket.ConnectError,"connect repeat");
        }
        else if (clientSocket == null || !clientSocket.Connected)
        {

            clientSocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);

            IPAddress ipAdress = IPAddress.Parse(ip);

            IPEndPoint endPoint = new IPEndPoint(ipAdress,port);

           IAsyncResult   connect =     clientSocket.BeginConnect(endPoint, ConnectCallBack,clientSocket);

            // 超时处理
           if (!WriteDot(connect))
           {
               this.callBackConnect(false, errorSocket, "连接超时");
           }



        }


    }

    private void ConnectCallBack(IAsyncResult asconnect)
    {

        try
        {
            clientSocket.EndConnect(asconnect);


            if (clientSocket.Connected == false)
            {

                errorSocket = ErrorSocket.ConnectUnSucessUnKnow;

                this.callBackConnect(false, errorSocket, "连接超时");

                return;
            }
            else
            {
               //  接受消息
                errorSocket = ErrorSocket.ConnnectSucess;
                this.callBackConnect(true, errorSocket, "sucess");

            }

        }
        catch (Exception  e)
        {

            this.callBackConnect(false,errorSocket,e.ToString());
        }
 
    }

    #endregion



    #region   Recv


    byte[] recvBuf;




    public void Recive()
    {

        if (clientSocket != null && clientSocket.Connected)
        {

           IAsyncResult  recv =   clientSocket.BeginReceive(recvBuf, 0, recvBuf.Length, SocketFlags.None, ReciveCallBack,clientSocket);



           if (!WriteDot(recv))
            {

                callBackRecv(false, ErrorSocket.RecvUnSucessUnKown, "recv  false", null, "");
            }



        }
    }


    private void ReciveCallBack( IAsyncResult  ar)
    {

        try
        {


            if (!clientSocket.Connected)
            {
                callBackRecv(false,ErrorSocket.RecvUnSucessUnKown,"connect false",null,"");

                return;
            }

            int length = clientSocket.EndReceive(ar);

            if (length == 0)
                return;


            recvBuffer.RecvByte(recvBuf,length);



        }
        catch( Exception  e )
        {

            callBackRecv(false, ErrorSocket.RecvUnSucessUnKown, e.ToString(),null,"");

        }

        Recive();

 
    }


    #endregion

    #region  RecvMsgOver



    public void RecvMsgOver(byte[] allByte)
    {

        callBackRecv(true, ErrorSocket.Sucess, "", null, "recv back sucess");

    }


    #endregion



    #region  Send 

    public void SendCallBack(IAsyncResult  ar)
    {

        try
        {

            int byteSend = clientSocket.EndSend(ar);


            if (byteSend > 0)
            {
                callBackSend(true, ErrorSocket.SendSucess, "");
            }
            else
            {
                callBackSend(false, ErrorSocket.SendUnSucessUnKown, "");
            }
           

        }
        catch (Exception e)
        {
            callBackSend(false, ErrorSocket.SendUnSucessUnKown, "");

        }




    }

    public void AsynSend(byte[] sendBuffer)
    {

        errorSocket = ErrorSocket.Sucess;

   

        if (clientSocket == null)
        {
            this.callBackSend(false,ErrorSocket.SocketNull,"");


        }
        else if (!clientSocket.Connected)
        {

            callBackSend(false, ErrorSocket.SocketUnConnect, "");
        }
        else
        {
            IAsyncResult  asySend=   clientSocket.BeginSend(sendBuffer, 0, sendBuffer.Length, SocketFlags.None, SendCallBack, clientSocket);

            // 超时处理
            if (!WriteDot(asySend))
            {

                callBackSend(false,ErrorSocket.SendUnSucessUnKown,"send failed");
            }



        }


    }

    #endregion  

    #region  Timeout  check

    // true 表示 可以写入 读取  false 表示超时
    bool WriteDot(IAsyncResult ar)
    {
        int i =0 ;
        // 是否 完成 
        while(ar.IsCompleted == false)
        {
             i++ ;
             if (i > 20)
             {
                 errorSocket = ErrorSocket.TimeOut;

                 return false;
             }

             Thread.Sleep(100);
        }

        return true;
    }


    #endregion



    #region   Disconnect

    //   跟服务器 断开连接
    public void DisconnectCallBack(IAsyncResult ar)
    {

        try
        {
            clientSocket.EndDisconnect(ar);


            clientSocket.Close();

            clientSocket = null;


            this.callBackDisconnect(true, ErrorSocket.DisConnectSucess, "");

        }
        catch (Exception e)
        {

            this.callBackDisconnect(false, ErrorSocket.DisConnectUnkown,e.ToString());
        }

    }


    public void AsyncDisconnect()
    {

        try
        {
            errorSocket = ErrorSocket.Sucess;

           // this.callBackDisconnect = tmpDisconnectBack;


            if (clientSocket == null)
            {
                this.callBackDisconnect(false,ErrorSocket.DisConnectUnkown,"client is null");
            }
            else if (!clientSocket.Connected)
            {
                this.callBackDisconnect(false, ErrorSocket.DisConnectUnkown, "client is unconnect");
            }
            else
            {



              IAsyncResult  asynDisconnect =   clientSocket.BeginDisconnect(false, DisconnectCallBack,clientSocket);


              if (!WriteDot(asynDisconnect))
                {

                    callBackDisconnect(false, ErrorSocket.DisConnectUnkown, "disconnect failed");
                }



            }

        }

        catch (Exception e)
        {
            callBackDisconnect(false, ErrorSocket.DisConnectUnkown, "disconnect failed");
        }
 
    }


    #endregion



}
