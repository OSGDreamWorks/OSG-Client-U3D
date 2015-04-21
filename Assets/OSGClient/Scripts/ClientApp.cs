using UnityEngine;
using System;
using System.Collections;
using System.Threading;
using protobuf;

public class ClientApp : MonoBehaviour {
	
	public static ClientApp app;

	private NetworkInterface networkInterface_;
	private Thread t_ = null;
	public NetThread thread = null;
	
	public string baseappIP = "127.0.0.1";
	public UInt16 baseappPort = 7850;
	public bool isbreak = false;

	// Use this for initialization
	void Start () {
		app = this;
		networkInterface_ = new NetworkInterface(this);
		thread = new NetThread(this);
		t_ = new Thread(new ThreadStart(thread.run));
		t_.Start();
		if(!networkInterface_.connect(baseappIP, baseappPort))
		{
			Debug.Log(string.Format("ClientApp::login_baseapp(): connect {0}:{1} is error!", baseappIP, baseappPort));
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void process()
	{
		while(!isbreak)
		{
			networkInterface_.process();
			sendTick();
		}
		
		Debug.Log("ClientApp::process(): break!");
	}
	
	public void sendTick()
	{
		//Debug.Log(string.Format("ClientApp::sendTick() : {0}", System.DateTime.Now.Ticks));
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

	public void OnSyncLoginInfo(ProtoBuf.IExtensible response)
	{
		LoginInfo info = (LoginInfo)response;
		Debug.Log(string.Format("ClientApp::OnSyncLoginInfo() -> serverIp = {0}", info.serverIp));
		
		string[] sArray=info.serverIp.Split(':');
		if(sArray.Length == 2 && !networkInterface_.connect(sArray[0], int.Parse(sArray[1])))
		{
			Debug.Log(string.Format("ClientApp::login_baseapp(): connect {0}:{1} is error!", baseappIP, baseappPort));
		}
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
