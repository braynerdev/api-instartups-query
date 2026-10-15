SELECT
    p.id,
    p.tipo_perfil::int AS tipo_perfil,
    p.nome,
    u."UserName" AS usuario,
    p.imagem_perfil_url,
    p.imagem_fundo_url,
    ST_Y(p.coordenada::geometry) AS latitude,
    ST_X(p.coordenada::geometry) AS longitude,
    p.total_seguidores,
    p.total_seguindo,
    p.total_curtidas,
    p.ativo,
    p.data_criacao,
    s.pitch,
    s.data_fundacao,
    s.tamanho_equipe::int AS tamanho_equipe,
    s.valor_buscado,
    s.setor::int AS setor,
    i.tese_investimento,
    i.ticket_minimo,
    i.ticket_maximo
FROM public.perfis p
LEFT JOIN public.startups s ON s.perfil_id = p.id
LEFT JOIN public.investidores i ON i.perfil_id = p.id
INNER JOIN auth."AspNetUsers" u ON u."Id" = p.usuario_id::text
WHERE p.usuario_id = @UsuarioId
LIMIT 1;
