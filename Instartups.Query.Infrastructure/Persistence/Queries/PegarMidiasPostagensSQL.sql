SELECT
    m.id,
    m.postagem_id,
    m.url_midia,
    m.tipo_midia
FROM public.midias_postagens m
WHERE m.postagem_id = ANY(@PostagemIds)
ORDER BY m.data_criacao, m.id;
