#include "stdafx.h"
#include "CLRDemoServer.h"
#include <iostream>
using namespace std;
using  namespace CLRDemoServer;
void main()
{

	CLR_DemoServer^ demo = CLR_DemoServer::GetInstance();

	cout << " ok " << endl;
	cin.get();
}