using UnityEngine;
using System.Collections;

using System.Collections.Generic;

using System.Threading;


public class NetWorkToServer
{

    private Queue<NetMsg> receiveMsgPool = new Queue<NetMsg>();
    private Queue<NetMsg> sendMsgPool = new Queue<NetMsg>();
    private NetSocket clientSocket;
    private Thread sendThread;



    private NetSocket.CallBackNormal m_callBackNormal;
    private NetSocket.CallBackNormal m_callBackConnect;
    private NetSocket.CallBackNormal m_callBackSend;
    private NetSocket.CallBackRecv m_callBackReceive;
    private NetSocket.CallBackNormal m_callBackDisConnect;



    //public NetWorkToServer(string ip, ushort port)
    //{
    //    clientSocket = new NetSocket();
    //    clientSocket.AsyncConnect(ip, port, ConnectCallBack, ReceiveCallBack);
    //    //clientSocket.AsyncDisconnect(DisConnectCallBack);
    //}


    public NetWorkToServer(string ip, ushort port, NetSocket.CallBackNormal connectFaled, NetSocket.CallBackNormal sendFailed, NetSocket.CallBackRecv recvFailed, NetSocket.CallBackNormal disConnect)
    {

        this.m_callBackConnect = connectFaled;

        this.m_callBackSend = sendFailed;

        m_callBackReceive = recvFailed;

        m_callBackDisConnect = disConnect;

        clientSocket = new NetSocket();
        clientSocket.AsyncConnnect(ip, port, connectFaled, sendFailed, recvFailed, disConnect);
        //clientSocket.AsyncDisconnect(DisConnectCallBack);
    }


    public bool IsConnect()
    {

        return clientSocket.IsConnect();
    }

    public void Disconnect()
    {
        clientSocket.AsyncDisconnect();
    }

    #region  网络方法回调


    private void ConnectCallBack(bool isSuccess, NetSocket.ErrorSocket error, string expection)
    {
        if (isSuccess)
        {
            sendThread = new Thread(LoopSending);
            sendThread.Start();


        }
        else
        {
            this.m_callBackConnect(isSuccess, error, expection);
        }




    }
    private void ReceiveCallBack(bool isSuccess,NetSocket.ErrorSocket  error, string expection, byte[] byteData, string strMsg)
    {
        if (isSuccess)
        {
            ReceiveMsgToNetMsg(byteData);
        }
        else
        {

            //  Debug.Log(error + expection);
            m_callBackReceive(isSuccess, error, expection, byteData, strMsg);
        }

        // Debug.Log(error + expection);
    }
    private void SendCallBack(bool isSuccess, NetSocket.ErrorSocket error, string expection)
    {
        if (!isSuccess)
        {
            m_callBackSend(isSuccess, error, expection);
        }




        //Debug.Log("send =="+error + expection);
        //Debug.Log(isSuccess);


    }

    private void DisConnectCallBack(bool isSuccess, NetSocket.ErrorSocket error, string expection)
    {
        if (isSuccess)
        {
            sendThread.Abort();

        }
        else
        {
            this.m_callBackDisConnect(isSuccess, error, expection);
            //Debug.Log(error + expection);
        }

        //  Debug.Log(error + expection);
    }


    #endregion

    #region 其他方法


    public void PutSendMessageToPool(NetMsg msg)
    {
        lock (sendMsgPool)
        {
            sendMsgPool.Enqueue(msg);
        }
    }

    public void Update()
    {
        if (receiveMsgPool != null && receiveMsgPool.Count > 0)
        {



            while (receiveMsgPool.Count > 0)
            {
                AnalyseData(receiveMsgPool.Dequeue());
            }


        }
    }

    ///TODO :ADD MORE

    /// <summary>
    /// 发送到上层
    /// </summary>
    /// <param name="msg"></param>
    private void AnalyseData(NetMsg tmpMsg)
    {
        //MsgCenter.instance.SendToMsg(msg);

       // NetMsg tmpMsg = receiveMsgPool.Dequeue();

        while (tmpMsg != null)
        {

            tmpMsg = receiveMsgPool.Dequeue();

          //  MsgCenter.instance.SendToMsg(msg);
            
        }
    }
    /// <summary>
    /// 转换为网络数据,并加入队列
    /// </summary>
    /// <param name="data"></param>
    private void ReceiveMsgToNetMsg(byte[] data)
    {
        NetMsg tmp = new NetMsg(data);

        //Debug.Log("socket    recv 1111==" + tmp.msgId);

        receiveMsgPool.Enqueue(tmp);


    }

    private void LoopSending()
    {
        while (clientSocket != null && clientSocket.IsConnect())
        {
            lock (sendMsgPool)
            {
                while (sendMsgPool.Count > 0)
                {
                    NetMsg msg = sendMsgPool.Dequeue();
                    clientSocket.AsynSend(msg.GetNetBytes());
                }
            }
            Thread.Sleep(100);
        }
    }


    #endregion
}