SELECT
    po.id,
    po.descricao,
    po.data_criacao,
    po.total_curtidas,
    EXISTS (
        SELECT 1
        FROM public.curtidas_postagens cp
        WHERE cp.postagem_id = po.id
          AND cp.perfil_autor_curtida_id = eu.id
    ) AS curtida_por_mim,
    po.ativo,
    a.id AS autor_id,
    a.nome AS autor_nome,
    a.tipo_perfil::int AS autor_tipo_perfil,
    a.imagem_perfil_url AS autor_imagem_perfil_url,
    s.setor::int AS autor_setor,
    ROUND((ST_Distance(a.coordenada, eu.coordenada) / 1000)::numeric, 1)::float8 AS autor_distancia_km,
    (a.id = eu.id) AS eh_meu
FROM public.postagens po
INNER JOIN public.perfis a ON a.id = po.autor_id
INNER JOIN public.perfis eu ON eu.id = @MeuPerfilId
LEFT JOIN public.startups s ON s.perfil_id = a.id
WHERE po.id = @PostagemId
  AND ((po.ativo AND a.ativo) OR a.id = eu.id)
LIMIT 1;
