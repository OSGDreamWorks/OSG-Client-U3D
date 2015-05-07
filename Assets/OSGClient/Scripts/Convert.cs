using UnityEngine;
using protobuf;

namespace protobuf
{
	public class Convert {
		static public protobuf.Transform FromU3DTransform(UnityEngine.Transform u3d)
		{
			protobuf.Transform trans = new protobuf.Transform ();
			trans.position = FromU3DVector3 (u3d.position);
			trans.rotation = FromU3DQuaternion (u3d.rotation);
			trans.scale = FromU3DVector3 (u3d.localScale);
			return trans;
		}
		static public protobuf.Quaternion FromU3DQuaternion(UnityEngine.Quaternion u3d)
		{
			protobuf.Quaternion quat = new protobuf.Quaternion ();
			quat.X = u3d.x;
			quat.Y = u3d.y;
			quat.Z = u3d.z;
			quat.W = u3d.w;
			return quat;
		}
		static public protobuf.Vector3 FromU3DVector3(UnityEngine.Vector3 u3d)
		{
			protobuf.Vector3 vec3 = new protobuf.Vector3 ();
			vec3.X = u3d.x;
			vec3.Y = u3d.y;
			vec3.Z = u3d.z;
			return vec3;
		}
	}
}