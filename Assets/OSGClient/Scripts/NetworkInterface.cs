using UnityEngine;
using System; 
using System.Net.Sockets; 
using System.Net; 
using System.Threading;
using protobuf;


public class NetworkInterface {
	
	public const int TCP_PACKET_MAX = 1460;
	public const int BUFFER_MAX = 1460 * 4;
	
	private Socket socket_ = null;
	private ClientApp app_ = null;
	private static ManualResetEvent TimeoutObject = new ManualResetEvent(false);
	private static byte[] _datas = new byte[BUFFER_MAX];
	
	public NetworkInterface(ClientApp app)
	{
		this.app_ = app;
	}
	
	public void reset()
	{
		if(valid())
			close();
		
		socket_ = null;
		TimeoutObject.Set();
	}
	
	public Socket sock()
	{
		return socket_;
	}
	
	public bool valid()
	{
		return ((socket_ != null) && (socket_.Connected == true));
	}
	
	public void close()
	{
		socket_.Close(0);
		socket_ = null;
	}
	
	public void process() 
	{
		if(socket_ != null && socket_.Connected)
		{
			recv();
		}
		else
		{
			System.Threading.Thread.Sleep(1);
		}
	}
	
	private static void connectCB(IAsyncResult asyncresult)
	{
		if(ClientApp.app.networkInterface().valid())
			ClientApp.app.networkInterface().sock().EndConnect(asyncresult);

		TimeoutObject.Set();

	}
	
	
	public bool connect(string ip, int port) 
	{
		int count = 0;
	__RETRY:
			reset();

		TimeoutObject.Reset();

		socket_ = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); 
		socket_.SetSocketOption (System.Net.Sockets.SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, BUFFER_MAX);
		
		try 
		{ 
			IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(ip), port); 
			
			socket_.BeginConnect(endpoint, new AsyncCallback(connectCB), socket_);
			
			if (TimeoutObject.WaitOne(10000))
			{
			}
			else
			{
				reset();
			}
			
		} 
		catch (Exception e) 
		{
			Debug.LogWarning("Exception : " + e.ToString());
			
			if(count < 3)
			{
				Debug.LogWarning("connect(" + ip + ":" + port + ") is error, try=" + (count++) + "!");
				goto __RETRY;
			}
			
			return false;
		} 
		
		if(!valid())
		{
			return false;
		}

		return true;
	}
	
	public void send(byte[] datas)
	{
		if(socket_ == null || socket_.Connected == false) 
		{
			throw new ArgumentException ("invalid socket!");
		}
		
		if (datas == null || datas.Length == 0 ) 
		{
			throw new ArgumentException ("invalid datas!");
		}
		
		try
		{
			socket_.Send(datas);
		}
		catch (SocketException err)
		{
			if (err.ErrorCode == 10054 || err.ErrorCode == 10053)
			{
				Debug.Log(string.Format("NetworkInterface::send(): disable connect!"));
				
				if(socket_ != null && socket_.Connected)
					socket_.Close();
				
				socket_ = null;
			}
			else{
				Debug.LogError(string.Format("NetworkInterface::send(): socket error(" + err.ErrorCode + ")!"));
			}
		}
	}
	
	public void recv()
	{
		if(socket_ == null || socket_.Connected == false) 
		{
			throw new ArgumentException ("invalid socket!");
		}
		
		if (socket_.Poll(100000, SelectMode.SelectRead))
		{
			if(socket_ == null || socket_.Connected == false) 
			{
				Debug.LogWarning("invalid socket!");
				return;
			}
			
			int successReceiveBytes = 0;
			
			try
			{
				successReceiveBytes = socket_.Receive(_datas, BUFFER_MAX, 0);
			}
			catch (SocketException err)
			{
				if (err.ErrorCode == 10054 || err.ErrorCode == 10053)
				{
					Debug.Log(string.Format("NetworkInterface::recv(): disable connect!"));
					
					if(socket_ != null && socket_.Connected)
						socket_.Close();
					
					socket_ = null;
				}
				else{
					Debug.LogError(string.Format("NetworkInterface::recv(): socket error(" + err.ErrorCode + ")!"));
				}

				return;
			}
			
			if(successReceiveBytes > 0)
			{
				//	Dbg.DEBUG_MSG(string.Format("NetworkInterface::recv(): size={0}!", successReceiveBytes));
			}
			else if(successReceiveBytes == 0)
			{
				Debug.Log(string.Format("NetworkInterface::recv(): disable connect!"));
				if(socket_ != null && socket_.Connected)
					socket_.Close();
				socket_ = null;

			}
			else
			{
				Debug.LogError(string.Format("NetworkInterface::recv(): socket error!"));
				
				if(socket_ != null && socket_.Connected)
					socket_.Close();
				socket_ = null;

				return;
			}
			int temp = 0;
			int size = 0;
			if(_datas.Length >= 4) {
				for(int i = 3; i >= 0; i--) {
					size <<= 8;
					temp = _datas[i] & 0xff;
					size |= temp;
				}
			}

			System.IO.MemoryStream request_stm = new System.IO.MemoryStream(size);
			request_stm.Write(_datas, 4, size);
			request_stm.Position = 0;
			Request request = ProtoBuf.Serializer.Deserialize<Request>(request_stm);

			Debug.Log(string.Format("NetworkInterface::recv(): request->{0}", request.method));

			string[] sArray=request.method.Split('.');
			if (sArray.Length == 2) {
				Type appType = app_.GetType();
				System.Reflection.MethodInfo methodInfo = appType.GetMethod(string.Format("OnSync{0}", sArray[1]));

				if (methodInfo != null) {
					Type protoType = Type.GetType(request.method);
					System.IO.MemoryStream proto_stm = new System.IO.MemoryStream(request.serialized_request);

					System.Reflection.MethodInfo method = typeof(ProtoBuf.Serializer).GetMethod("Deserialize");
					System.Reflection.MethodInfo generic = method.MakeGenericMethod(protoType);
					object[] param = new object[] {proto_stm};
					ProtoBuf.IExtensible protoTypeObj = (ProtoBuf.IExtensible)generic.Invoke(this, param);
					
					object[] paramProto = new object[] {protoTypeObj};
					methodInfo.Invoke(app_, paramProto);
				}else {
					Debug.LogError(string.Format("NetworkInterface::recv(): Reflection {0} not exist!", request.method));
				}

			}
			else
			{
				Debug.LogError(string.Format("NetworkInterface::recv(): method {0} error!", request.method));

			}
		}
	}
}
