SELECT
    p.id,
    p.tipo_perfil::int AS tipo_perfil,
    p.nome,
    p.imagem_perfil_url,
    u."UserName" AS usuario,
    s.setor::int AS setor,
    left(coalesce(s.pitch, i.tese_investimento), 140) AS resumo,
    ROUND((ST_Distance(p.coordenada, eu.coordenada) / 1000)::numeric, 1)::float8 AS distancia_km,
    EXISTS (
        SELECT 1
        FROM public.perfil_seguidores ps
        WHERE ps.seguidor_id = eu.id
          AND ps.seguido_id = p.id
    ) AS seguindo_por_mim,
    (p.id = eu.id) AS eh_meu,
    CASE
        WHEN u."NormalizedEmail" = @Email THEN 'email'
        WHEN p.nome ILIKE '%' || @Termo || '%' THEN 'nome'
        ELSE 'usuario'
    END AS encontrado_por
FROM public.perfis p
INNER JOIN auth."AspNetUsers" u ON u."Id" = p.usuario_id::text
INNER JOIN public.perfis eu ON eu.id = @MeuPerfilId
LEFT JOIN public.startups s ON s.perfil_id = p.id
LEFT JOIN public.investidores i ON i.perfil_id = p.id
WHERE p.ativo
  AND (@TipoPerfil::int IS NULL OR p.tipo_perfil = @TipoPerfil::int)
  AND (
        p.nome ILIKE '%' || @Termo || '%'
     OR u."UserName" ILIKE '%' || @Termo || '%'
     OR u."NormalizedEmail" = @Email
  )
  AND (@CursorNome::text IS NULL OR (p.nome, p.id) > (@CursorNome::text, @CursorId::uuid))
ORDER BY p.nome, p.id
LIMIT @Limite;
