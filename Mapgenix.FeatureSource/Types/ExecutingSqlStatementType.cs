namespace Mapgenix.FeatureSource
{
    /// <summary>
    /// Parameters passed in through the ExecutingSqlStatement event in MsSql2008FeatureSource.
    /// </summary>
    public enum ExecutingSqlStatementType
    {
        GetFeaturesByIds = 1,
        GetFeaturesOutsideBoundingBox = 3,
        GetSpatialDataType = 4,
        GetBoundingBox = 5,
        GetAllFeatures = 6,
        GetCount = 7,
        GetColumns = 8,
        BuildIndex = 9,
        ExecuteScalar = 10,
        ExecuteQuery = 11,
        ExecuteNonQuery = 12,
        GetFirstGeometryType = 13,
        MakeAllGeometriesValid = 14,
        Validate = 15,
        CommitTransactionEx = 16,
        GetFeaturesInsideBoundingBoxEx = 17,
        Unknown = 18
    }
}
