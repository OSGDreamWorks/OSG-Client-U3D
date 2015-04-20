using UnityEngine;
using System;
using System.Collections;
using System.Threading;

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
}
