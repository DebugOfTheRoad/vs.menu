namespace csharp Sodao.Urljump.Service.Thrift

/**
* User����
*/
struct User
{
	1:i64 UserId;// userid
	2:string UserName;// ע���˺�
	3:string NickName;// �û��ǳ�
	4:i32 Gender;// �Ա�1���У�2��Ů��0��δ֪��
	5:i64 RegTime;// ע��ʱ��
}

/*service*/
service UrljumpService{
	/**
	* User_GetVoidʹ�÷���
	*/
	 void User_GetVoid();

	 /**
	  * User_Getʹ�÷���
	  * @param i64 userId - the string to print
	  * @return User - returns the i8/byte 'thing'
	  */
	 User User_Get(1:i64 userId);//����uid��ȡ�û���Ϣ

	 /**
	 * User_GetByNameʹ�÷���
	 */
	 User User_GetByName(1:i64 userId, 2:string userName);
}