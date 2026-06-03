using System.Collections.Generic;
using UnityEngine;

public class BookOrbitWeapon : MonoBehaviour
{
    public Sprite bookSprite;
    public int damage = 1;
    public int level;
    public int maxLevel = 10;
    public int bookCount;
    public int maxBookCount = 7;
    public float radius = 1.55f;
    public float rotationSpeed = 170f;
    public float hitCooldown = 0.38f;

    private readonly List<GameObject> books = new();
    private Transform orbitRoot;

    private void Awake()
    {
        orbitRoot = new GameObject("Book Orbit Root").transform;
        orbitRoot.SetParent(transform, false);
    }

    private void Update()
    {
        if (bookCount <= 0)
            return;

        EnsureBooks();
        orbitRoot.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    public void Upgrade()
    {
        level = Mathf.Min(maxLevel, level + 1);
        bookCount = Mathf.Clamp(1 + level / 2, 1, maxBookCount);
        damage = 1 + level / 4;
        radius = Mathf.Min(2.15f, radius + 0.04f);
        rotationSpeed = Mathf.Min(290f, rotationSpeed + 8f);
        EnsureBooks();
    }

    public bool IsMaxLevel()
    {
        return level >= maxLevel;
    }

    private void EnsureBooks()
    {
        while (books.Count < bookCount)
            books.Add(CreateBook(books.Count));

        for (var i = 0; i < books.Count; i++)
        {
            var active = i < bookCount;
            books[i].SetActive(active);
            if (!active)
                continue;

            var angle = i * Mathf.PI * 2f / bookCount;
            books[i].transform.localPosition = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;

            var damageZone = books[i].GetComponent<OrbitDamageZone>();
            damageZone.damage = damage;
            damageZone.hitCooldown = hitCooldown;
        }
    }

    private GameObject CreateBook(int index)
    {
        var book = new GameObject("Orbit Book " + (index + 1));
        book.transform.SetParent(orbitRoot, false);
        book.transform.localScale = Vector3.one * 0.8f;

        var sr = book.AddComponent<SpriteRenderer>();
        sr.sprite = bookSprite;
        sr.sortingOrder = 8;

        var collider = book.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.36f;

        var damageZone = book.AddComponent<OrbitDamageZone>();
        damageZone.damage = damage;
        damageZone.hitCooldown = hitCooldown;
        return book;
    }
}
