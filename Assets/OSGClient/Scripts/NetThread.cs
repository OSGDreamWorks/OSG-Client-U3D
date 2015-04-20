using UnityEngine;
using System; 
using System.Collections;

public class NetThread {
	
	ClientApp app_;
	public bool over = false;
	
	public NetThread(ClientApp app)
	{
		this.app_ = app;
	}
	
	public void run()
	{
		Debug.Log("NetThread::run()");
		int count = 0;

START_RUN:

			over = false;
		
		try
		{
			this.app_.process();
			count = 0;
		}
		catch (Exception e)
		{
			Debug.Log("Exception");
			Debug.Log("NetThread::try run:" + count);
			
			count ++;
			if(count < 10)
				goto START_RUN;
		}
		
		over = true;
		Debug.Log("NetThread::end()");
	}
}
