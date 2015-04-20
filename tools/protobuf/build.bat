@echo off

echo =============================================================
echo "start build CSharp Protobuf"
protogen -i:msg.proto -o:Msg.cs
echo OK
move Msg.cs ..\..\Assets\OSGClient\Scripts\Msg.cs
echo =============================================================
