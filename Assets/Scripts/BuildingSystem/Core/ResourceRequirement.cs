namespace BuildingSystem.Core
{
    [System.Serializable]
    public class ResourceRequirement
    {
        public string resourceId;
        public int amount;

        public ResourceRequirement(string id, int amt)
        {
            resourceId = id;
            amount = amt;
        }
    }
}