//using CookComputing.XmlRpc;

//for (maybe needed in the future) using moodle-webservices
namespace Moodler
{
    public interface IMoodle //: IXmlRpcProxy
    {
       // [XmlRpcMethod("moodle_user_get_users_by_id")]
        object[] GetUserById(object[] id);
        //[XmlRpcMethod("moodle_course_get_courses")]
        object[] GetCourses();
    }
}