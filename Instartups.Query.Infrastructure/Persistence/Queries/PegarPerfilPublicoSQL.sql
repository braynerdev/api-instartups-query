WITH perfil_logado AS (
    SELECT
        pl.id,
        pl.coordenada
    FROM public.perfis pl
    WHERE pl.usuario_id = @UsuarioId
),
perfil_busca AS (
    SELECT
        pb.id,
        pb.tipo_perfil,
        pb.nome,
        pb.usuario_id,
        pb.imagem_perfil_url,
        pb.imagem_fundo_url,
        pb.coordenada,
        pb.total_seguidores,
        pb.total_seguindo,
        pb.total_curtidas,
        pb.ativo,
        pb.data_criacao
    FROM public.perfis pb
    LEFT JOIN perfil_logado pl ON TRUE
    WHERE pb.id = @PerfilId
      AND (pb.ativo OR pb.id = pl.id)
),
usuario_busca AS (
    SELECT
        u."Id" AS id,
        u."UserName" AS usuario
    FROM auth."AspNetUsers" u
    INNER JOIN perfil_busca pb ON u."Id" = pb.usuario_id::text
),
seguindo AS (
    SELECT EXISTS (
        SELECT 1
        FROM public.perfil_seguidores ps
        INNER JOIN perfil_logado pl ON ps.seguidor_id = pl.id
        WHERE ps.seguido_id = @PerfilId
    ) AS seguindo_por_mim
),
startup AS (
    SELECT
        s.perfil_id,
        s.pitch,
        s.data_fundacao,
        s.tamanho_equipe,
        s.valor_buscado,
        s.setor
    FROM public.startups s
    WHERE s.perfil_id = @PerfilId
),
investidor AS (
    SELECT
        i.perfil_id,
        i.tese_investimento,
        i.ticket_minimo,
        i.ticket_maximo
    FROM public.investidores i
    WHERE i.perfil_id = @PerfilId
)
SELECT
    pb.id,
    pb.tipo_perfil::int AS tipo_perfil,
    pb.nome,
    ub.usuario,
    pb.imagem_perfil_url,
    pb.imagem_fundo_url,
    ROUND((ST_Distance(pb.coordenada, pl.coordenada) / 1000)::numeric, 1)::float8 AS distancia_km,
    sg.seguindo_por_mim,
    pb.total_seguidores,
    pb.total_seguindo,
    pb.total_curtidas,
    pb.ativo,
    pb.data_criacao,
    s.pitch,
    s.data_fundacao,
    s.tamanho_equipe::int AS tamanho_equipe,
    s.valor_buscado,
    s.setor::int AS setor,
    i.tese_investimento,
    i.ticket_minimo,
    i.ticket_maximo
FROM perfil_busca pb
INNER JOIN usuario_busca ub ON ub.id = pb.usuario_id::text
LEFT JOIN perfil_logado pl ON TRUE
CROSS JOIN seguindo sg
LEFT JOIN startup s ON s.perfil_id = pb.id
LEFT JOIN investidor i ON i.perfil_id = pb.id
LIMIT 1;