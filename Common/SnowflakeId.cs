using Yitter.IdGenerator;

namespace EnterpriseMS.Common;

public static class SnowflakeId
{
    static SnowflakeId()
    {
        YitIdHelper.SetIdGenerator(new IdGeneratorOptions(1));
    }
    public static long Next() => YitIdHelper.NextId();
}
