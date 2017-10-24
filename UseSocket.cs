using UnityEngine;

using System.Collections;


//
//  UseSocket#FILEEXTENSION#
//  #PROJECTNAME#
//
//  Created by #SMARTDEVELOPERS# on #CREATIONDATE#.
//
//

public class UseSocket : MonoBehaviour 
{


    NetWorkToServer socket;




    public  void  ConnectCallBack(bool sucess, NetSocket.ErrorSocket  error, string exception)
{

}



    public void SendCallBack(bool sucess, NetSocket.ErrorSocket error, string exception)
    {

        ///   
    }


    public void DisConnectCallBack(bool sucess, NetSocket.ErrorSocket error, string exception)
    {

    }



    public void CallBackRecv(bool sucess, NetSocket.ErrorSocket error, string exception, byte[] byteMessage, string strMessag)
{

}



// Use this for initialization
	
void Start ()
 {


     socket = new NetWorkToServer("127.0.0.1",18001,ConnectCallBack,SendCallBack,CallBackRecv,DisConnectCallBack);
	
	
}
	
	
// Update is called once per frame
	
void Update () 
{


    if (Input.GetMouseButton(0))
    {
        byte[] tmpByte = System.Text.Encoding.UTF8.GetBytes("91u990u90u");


        test.TestMsg tmpMsg = new test.TestMsg();

        //byte[]  sendBytes=  ProtoTools.ProtoSerialize(tmpMsg);

        //NetMsg tmpMsg2 = new NetMsg(sendBytes, 1, 0);

        //socket.PutSendMessageToPool(tmpMsg2);

        
    }
}

}
