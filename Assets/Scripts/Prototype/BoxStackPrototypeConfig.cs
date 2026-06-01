// 프로토타입 설정 - 제품 데이터 에셋으로 넘어가기 위한 안전한 중간 단계.

using System;
using UnityEngine;

/// <summary>
/// BoxStack 프로토타입의 튜닝값과 스테이지 값을 담는 데이터 경계입니다.
/// </summary>
[CreateAssetMenu(fileName = "BoxStackPrototypeConfig", menuName = "BoxStack/Prototype Config")]
public sealed class BoxStackPrototypeConfig : ScriptableObject
{
    private static readonly TuningSettings DefaultTuning = new TuningSettings(
        baseMoveRange: 2.45f,
        baseMoveSpeed: 1.85f,
        moveRangeScreenPadding: 0.12f,
        dropMinimumResolveSeconds: 0.2f,
        dropStableSeconds: 0.2f,
        stackStopLinearVelocity: 0.05f,
        stackStopAngularVelocity: 5.0f,
        clearValidationSeconds: 5.0f,
        parcelFriction: 8.0f,
        floorFriction: 8.0f,
        parcelBounciness: 0f,
        settledGravityScale: 1.6f,
        droppingGravityScale: 1.25f,
        maxDroppingFallSpeed: 5.2f,
        dropPreContactVelocityResetDistance: 0.12f,
        linearDamping: 0.6f,
        angularDamping: 0.8f,
        lostHeight: -4.0f,
        lostHorizontalDistance: 4.0f);

    private static readonly StageSettings[] DefaultStages =
    {
        new StageSettings(1, 4, "BBBB", 0.75f, 0.75f),
        new StageSettings(2, 5, "BBBBB", 0.80f, 0.80f),
        new StageSettings(3, 6, "BBNBBB", 0.85f, 0.85f),
        new StageSettings(4, 6, "BNBBNB", 0.90f, 0.90f),
        new StageSettings(5, 7, "BBNBNBB", 0.95f, 0.95f),
        new StageSettings(6, 7, "NBBNBBN", 1.00f, 1.00f),
        new StageSettings(7, 8, "BBNBNBBB", 1.00f, 1.00f),
        new StageSettings(8, 8, "NBNBBNBN", 1.05f, 1.00f),
        new StageSettings(9, 8, "BNNBNBBN", 1.05f, 1.05f),
        new StageSettings(10, 9, "BBNNBNBBB", 1.10f, 1.05f),
        new StageSettings(11, 9, "NNBBNBNBB", 1.10f, 1.10f),
        new StageSettings(12, 9, "BNBNNBNBB", 1.15f, 1.10f),
        new StageSettings(13, 10, "BBNNBNBNBB", 1.15f, 1.15f),
        new StageSettings(14, 10, "NBNNBNBBNB", 1.20f, 1.15f),
        new StageSettings(15, 10, "BNNBBNSNBB", 1.20f, 1.20f),
        new StageSettings(16, 11, "NBBNSBNBNBB", 1.25f, 1.20f),
        new StageSettings(17, 11, "BNBSNBBNSBB", 1.25f, 1.25f),
        new StageSettings(18, 12, "BNSBBNSBNNBB", 1.30f, 1.25f),
        new StageSettings(19, 12, "NSBNNBBNSBNB", 1.35f, 1.30f),
        new StageSettings(20, 12, "BNSNBSNBSNBB", 1.40f, 1.30f)
    };

    [SerializeField] private TuningSettings tuning = DefaultTuning;
    [SerializeField] private StageSettings[] stages = DefaultStages;

    public TuningSettings Tuning
    {
        get { return tuning; }
    }

    /// <summary>
    /// 프로토타입에서 사용할 수 있는 설정 스테이지 수입니다.
    /// </summary>
    public int StageCount
    {
        get { return ActiveStages.Length; }
    }

    private StageSettings[] ActiveStages
    {
        get
        {
            if (stages == null || stages.Length == 0)
            {
                return DefaultStages;
            }

            return stages;
        }
    }

    /// <summary>
    /// 범위 안으로 보정한 인덱스의 스테이지 설정을 반환합니다.
    /// </summary>
    public StageSettings GetStage(int index)
    {
        StageSettings[] activeStages = ActiveStages;
        return activeStages[Mathf.Clamp(index, 0, activeStages.Length - 1)];
    }

    /// <summary>
    /// 설정 에셋이 없을 때 사용할 B013 튜닝 기준값을 반환합니다.
    /// </summary>
    public static TuningSettings GetDefaultTuning()
    {
        return DefaultTuning;
    }

    /// <summary>
    /// 설정 에셋이 없을 때 사용할 B013 스테이지 기준값을 반환합니다.
    /// </summary>
    public static StageSettings GetDefaultStage(int index)
    {
        return DefaultStages[Mathf.Clamp(index, 0, DefaultStages.Length - 1)];
    }

    /// <summary>
    /// 코드에 내장된 fallback 스테이지 수입니다.
    /// </summary>
    public static int DefaultStageCount
    {
        get { return DefaultStages.Length; }
    }

    [Serializable]
    public struct TuningSettings
    {
        public TuningSettings(
            float baseMoveRange,
            float baseMoveSpeed,
            float moveRangeScreenPadding,
            float dropMinimumResolveSeconds,
            float dropStableSeconds,
            float stackStopLinearVelocity,
            float stackStopAngularVelocity,
            float clearValidationSeconds,
            float parcelFriction,
            float floorFriction,
            float parcelBounciness,
            float settledGravityScale,
            float droppingGravityScale,
            float maxDroppingFallSpeed,
            float dropPreContactVelocityResetDistance,
            float linearDamping,
            float angularDamping,
            float lostHeight,
            float lostHorizontalDistance)
        {
            BaseMoveRange = baseMoveRange;
            BaseMoveSpeed = baseMoveSpeed;
            MoveRangeScreenPadding = moveRangeScreenPadding;
            DropMinimumResolveSeconds = dropMinimumResolveSeconds;
            DropStableSeconds = dropStableSeconds;
            StackStopLinearVelocity = stackStopLinearVelocity;
            StackStopAngularVelocity = stackStopAngularVelocity;
            ClearValidationSeconds = clearValidationSeconds;
            ParcelFriction = parcelFriction;
            FloorFriction = floorFriction;
            ParcelBounciness = parcelBounciness;
            SettledGravityScale = settledGravityScale;
            DroppingGravityScale = droppingGravityScale;
            MaxDroppingFallSpeed = maxDroppingFallSpeed;
            DropPreContactVelocityResetDistance = dropPreContactVelocityResetDistance;
            LinearDamping = linearDamping;
            AngularDamping = angularDamping;
            LostHeight = lostHeight;
            LostHorizontalDistance = lostHorizontalDistance;
        }

        public float BaseMoveRange;
        public float BaseMoveSpeed;
        public float MoveRangeScreenPadding;
        public float DropMinimumResolveSeconds;
        public float DropStableSeconds;
        public float StackStopLinearVelocity;
        public float StackStopAngularVelocity;
        public float ClearValidationSeconds;
        public float ParcelFriction;
        public float FloorFriction;
        public float ParcelBounciness;
        public float SettledGravityScale;
        public float DroppingGravityScale;
        public float MaxDroppingFallSpeed;
        public float DropPreContactVelocityResetDistance;
        public float LinearDamping;
        public float AngularDamping;
        public float LostHeight;
        public float LostHorizontalDistance;
    }

    [Serializable]
    public struct StageSettings
    {
        public StageSettings(int number, int targetBoxes, string boxSequence, float speedMultiplier, float rangeMultiplier)
        {
            Number = number;
            TargetBoxes = targetBoxes;
            BoxSequence = boxSequence;
            SpeedMultiplier = speedMultiplier;
            RangeMultiplier = rangeMultiplier;
        }

        public int Number;
        public int TargetBoxes;
        public string BoxSequence;
        public float SpeedMultiplier;
        public float RangeMultiplier;
    }
}
