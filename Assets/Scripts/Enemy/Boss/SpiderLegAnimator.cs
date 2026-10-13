using UnityEngine;
using DG.Tweening;

public class SpiderLegAnimator : MonoBehaviour
{
    [Header("Трипод А (Л1, П2, Л3)")]
    [SerializeField] private Transform[] tripodA;

    [Header("Трипод Б (П1, Л2, П3)")]
    [SerializeField] private Transform[] tripodB;

    [Header("Налаштування динаміки павука")]
    [SerializeField] private float stepHeight = 14f;     // Висота дуги підйому лапи
    [SerializeField] private float stepAngle = 18f;      // Довжина кроку (амплітуда)
    [SerializeField] private float stepDuration = 0.32f;  // Швидкість одного кроку

    private Quaternion[] defaultRotA;
    private Quaternion[] defaultRotB;

    private Sequence walkSequence;
    private Sequence prepareSequence;
    private bool isWalking = false;

    private void Awake()
    {
        defaultRotA = SaveRotations(tripodA);
        defaultRotB = SaveRotations(tripodB);
    }

    public void StartWalking()
    {
        if (isWalking) return;
        isWalking = true;

        // 1. ПОДГОТОВКА: Плавне розведення лап у протифазу БЕЗ ТЕЛЕПОРТАЦІЇ
        prepareSequence = DOTween.Sequence();
        prepareSequence.Append(SetTripodPose(tripodA, defaultRotA, offsetAngle: -stepAngle, duration: 0.15f));
        prepareSequence.Join(SetTripodPose(tripodB, defaultRotB, offsetAngle: stepAngle, duration: 0.15f));

        prepareSequence.OnComplete(() =>
        {
            if (!isWalking) return;

            // 2. БЕЗКІНЕЧНИЙ БЕЗШОВНИЙ ЦИКЛ (Кінцева позиція = Стартовій позиції)
            walkSequence = DOTween.Sequence();

            // Фаза 1: Трипод А здійснює політ ВПЕРЕД, Трипод Б штовхає НАЗАД по землі
            walkSequence.Append(AnimateSwing(tripodA, defaultRotA, stepDuration));
            walkSequence.Join(AnimatePush(tripodB, defaultRotB, stepDuration));

            // Фаза 2: Трипод Б здійснює політ ВПЕРЕД, Трипод А штовхає НАЗАД по землі
            walkSequence.Append(AnimateSwing(tripodB, defaultRotB, stepDuration));
            walkSequence.Join(AnimatePush(tripodA, defaultRotA, stepDuration));

            // У цій точці Трипод А знову ззаду, а Трипод Б попереду — цикл перезапускається ідеально гладко!
            walkSequence.SetLoops(-1, LoopType.Restart);
        });
    }

    public void StopWalking()
    {
        if (!isWalking) return;
        isWalking = false;

        if (prepareSequence != null && prepareSequence.IsActive()) prepareSequence.Kill();
        if (walkSequence != null && walkSequence.IsActive()) walkSequence.Kill();

        // М'який повернення всіх лап на землю в початкову стійку
        ResetTripod(tripodA, defaultRotA, 0.25f);
        ResetTripod(tripodB, defaultRotB, 0.25f);
    }

    // Органічна павуча дуга в повітрі: Імпульсний підйом ➔ М'яке приземлення
    private Sequence AnimateSwing(Transform[] legs, Quaternion[] defaultRots, float duration)
    {
        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < legs.Length; i++)
        {
            Transform leg = legs[i];
            Quaternion baseRot = defaultRots[i];
            float sideSign = GetSideSign(leg);

            // Проміжна позиція (лапа в повітрі у найвищій точці)
            Quaternion targetLift = baseRot * Quaternion.Euler(-stepHeight, (stepAngle * 0.1f) * sideSign, 0);

            // Кінцева позиція (лапа торкнулася землі попереду)
            Quaternion targetFront = baseRot * Quaternion.Euler(0, stepAngle * sideSign, 0);

            Sequence legSeq = DOTween.Sequence();
            // Швидкий гідравлічний відрив від землі
            legSeq.Append(leg.DOLocalRotateQuaternion(targetLift, duration * 0.45f).SetEase(Ease.OutQuad));
            // М'яке втикання в ґрунт
            legSeq.Append(leg.DOLocalRotateQuaternion(targetFront, duration * 0.55f).SetEase(Ease.InQuad));

            seq.Join(legSeq);
        }

        return seq;
    }

    // Плавний суцільний поштовх тулуба по землі
    private Sequence AnimatePush(Transform[] legs, Quaternion[] defaultRots, float duration)
    {
        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < legs.Length; i++)
        {
            Transform leg = legs[i];
            Quaternion baseRot = defaultRots[i];
            float sideSign = GetSideSign(leg);

            // Рівномірний рух з переднього положення в заднє
            Quaternion targetBack = baseRot * Quaternion.Euler(0, -stepAngle * sideSign, 0);

            seq.Join(leg.DOLocalRotateQuaternion(targetBack, duration).SetEase(Ease.Linear));
        }

        return seq;
    }

    private Sequence SetTripodPose(Transform[] legs, Quaternion[] defaultRots, float offsetAngle, float duration)
    {
        Sequence seq = DOTween.Sequence();
        for (int i = 0; i < legs.Length; i++)
        {
            float sideSign = GetSideSign(legs[i]);
            Quaternion target = defaultRots[i] * Quaternion.Euler(0, offsetAngle * sideSign, 0);
            seq.Join(legs[i].DOLocalRotateQuaternion(target, duration).SetEase(Ease.InOutSine));
        }
        return seq;
    }

    private void ResetTripod(Transform[] legs, Quaternion[] defaultRots, float duration)
    {
        for (int i = 0; i < legs.Length; i++)
        {
            if (legs[i] != null)
                legs[i].DOLocalRotateQuaternion(defaultRots[i], duration).SetEase(Ease.OutCubic);
        }
    }

    private Quaternion[] SaveRotations(Transform[] legs)
    {
        Quaternion[] rots = new Quaternion[legs.Length];
        for (int i = 0; i < legs.Length; i++)
        {
            if (legs[i] != null) rots[i] = legs[i].localRotation;
        }
        return rots;
    }

    // Автоматично враховує ліву/праву сторону для дзеркального кроку
    private float GetSideSign(Transform leg)
    {
        return leg.localPosition.x >= 0 ? 1f : -1f;
    }
}