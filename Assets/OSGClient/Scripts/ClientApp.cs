﻿using UnityEngine;
using System;
using System.Collections;
using System.Threading;
using protobuf;

public class ClientApp : MonoBehaviour {
	
	public static ClientApp app;

	private NetworkInterface networkInterface_;
	private Thread t_ = null;
	public NetThread thread = null;
	
	public string baseappIP = "127.0.0.1:7850";
	private string serverIP = "127.0.0.1:7900";
	public bool isbreak = false;
	private long countStart = (System.DateTime.Now.Ticks - 621355968000000000) / 10000;
	private long countEnd = (System.DateTime.Now.Ticks - 621355968000000000) / 10000;

	// Use this for initialization
	void Start () {
		app = this;
		networkInterface_ = new NetworkInterface(this);
		thread = new NetThread(this);
		t_ = new Thread(new ThreadStart(thread.run));
		t_.Start();
		Connect (baseappIP);
		Connect (serverIP);
		sendLogin ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void process()
	{
		while(!isbreak)
		{
			networkInterface_.process();
			if(networkInterface_.valid() && countEnd - countStart >= 1000)
			{
				countStart = (System.DateTime.Now.Ticks - 621355968000000000) / 10000;
				countEnd = (System.DateTime.Now.Ticks - 621355968000000000) / 10000;
				sendTick();
			}
			countEnd = (System.DateTime.Now.Ticks - 621355968000000000) / 10000;
		}
		
		Debug.Log("ClientApp::process(): break!");
	}
	
	public void sendTick()
	{
		Debug.Log ("sendTick");
		protobuf.Ping ping = new protobuf.Ping ();
		if (!networkInterface_.send ("Connector.Ping", ping)) {
			//CancelInvoke("sendTick");
		}
	}
	
	public void sendLogin()
	{
		Debug.Log ("sendLogin");
		
		if (Connect (serverIP)) {
			
			Login login = new Login ();
			login.account = "account";
			login.password = "password";
			networkInterface_.send ("Connector.Login", login);
			
			Debug.Log (string.Format ("ClientApp::login_baseapp(): connect {0} ", serverIP));
		} else {
			Debug.LogError (string.Format ("ClientApp::login_baseapp(): connect {0} is error!", serverIP));
		}
	}
	
	public void reset()
	{
	}
	
	public void OnDestroy()
	{
		Debug.Log("ClientApp::destroy()");
		isbreak = true;
		
		int i = 0;
		while(thread!=null && !thread.over && i < 50)
		{
			Thread.Sleep(1);
			i += 1;
		}
		
		if(t_ != null)
			t_.Abort();
		
		t_ = null;
		
		reset();
	}
	
	public NetworkInterface networkInterface(){
		return networkInterface_;
	}

	public bool Connect(string serverIp) {
		string[] sArray=serverIp.Split(':');
		if (sArray.Length == 2 && networkInterface_.connect (sArray [0], int.Parse (sArray [1]))) {
			return true;		
		} else {
			networkInterface_.reset();
			return false;
		}
	}

	public void OnConnect() {
		Debug.Log ("OnConnect");
	}

	public void OnSyncLoginInfo(ProtoBuf.IExtensible response)
	{
		LoginInfo info = (LoginInfo)response;
		Debug.Log(string.Format("ClientApp::OnSyncLoginInfo() -> serverIp = {0}", info.serverIp));
		serverIP = info.serverIp;
	}
	
	public void OnSyncPingResult(ProtoBuf.IExtensible response)
	{
		PingResult ping = (PingResult)response;
		Debug.Log(string.Format("ClientApp::OnSyncPingResult() -> server_time = {0}", ping.server_time));
	}
	
	public void OnSyncLoginResult(ProtoBuf.IExtensible response)
	{
		LoginResult login = (LoginResult)response;
		Debug.Log(string.Format("ClientApp::OnSyncLoginResult() -> sessionKey = {0}", login.sessionKey));
	}
}
