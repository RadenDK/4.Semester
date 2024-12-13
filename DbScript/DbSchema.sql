--
-- PostgreSQL database dump
--

-- Dumped from database version 17.0 (Debian 17.0-1.pgdg120+1)
-- Dumped by pg_dump version 17.0

-- Started on 2024-12-05 13:57:35

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 218 (class 1259 OID 21044)
-- Name: companies; Type: TABLE; Schema: public; Owner: sa
--

CREATE TABLE public.companies (
    id integer NOT NULL,
    name character varying(100)
);


ALTER TABLE public.companies OWNER TO sa;

--
-- TOC entry 217 (class 1259 OID 21043)
-- Name: companies_id_seq; Type: SEQUENCE; Schema: public; Owner: sa
--

CREATE SEQUENCE public.companies_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.companies_id_seq OWNER TO sa;

--
-- TOC entry 3429 (class 0 OID 0)
-- Dependencies: 217
-- Name: companies_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: sa
--

ALTER SEQUENCE public.companies_id_seq OWNED BY public.companies.id;


--
-- TOC entry 220 (class 1259 OID 21051)
-- Name: departments; Type: TABLE; Schema: public; Owner: sa
--

CREATE TABLE public.departments (
    id integer NOT NULL,
    name character varying(100),
    company_id integer
);


ALTER TABLE public.departments OWNER TO sa;

--
-- TOC entry 219 (class 1259 OID 21050)
-- Name: departments_id_seq; Type: SEQUENCE; Schema: public; Owner: sa
--

CREATE SEQUENCE public.departments_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.departments_id_seq OWNER TO sa;

--
-- TOC entry 3430 (class 0 OID 0)
-- Dependencies: 219
-- Name: departments_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: sa
--

ALTER SEQUENCE public.departments_id_seq OWNED BY public.departments.id;


--
-- TOC entry 228 (class 1259 OID 21118)
-- Name: foosball_matches; Type: TABLE; Schema: public; Owner: sa
--

CREATE TABLE public.foosball_matches (
    id integer NOT NULL,
    table_id integer,
    start_time timestamp without time zone DEFAULT (CURRENT_TIMESTAMP AT TIME ZONE 'Europe/Copenhagen'::text),
    end_time timestamp without time zone,
    red_team_id integer,
    blue_team_id integer,
    team_red_score integer DEFAULT 0,
    team_blue_score integer DEFAULT 0,
    valid_elo_match boolean
);


ALTER TABLE public.foosball_matches OWNER TO sa;

--
-- TOC entry 227 (class 1259 OID 21117)
-- Name: foosball_matches_id_seq; Type: SEQUENCE; Schema: public; Owner: sa
--

CREATE SEQUENCE public.foosball_matches_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.foosball_matches_id_seq OWNER TO sa;

--
-- TOC entry 3431 (class 0 OID 0)
-- Dependencies: 227
-- Name: foosball_matches_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: sa
--

ALTER SEQUENCE public.foosball_matches_id_seq OWNED BY public.foosball_matches.id;


--
-- TOC entry 224 (class 1259 OID 21084)
-- Name: foosball_tables; Type: TABLE; Schema: public; Owner: sa
--

CREATE TABLE public.foosball_tables (
    id integer NOT NULL,
    department_id integer,
    company_id integer,
    active_match_id integer
);


ALTER TABLE public.foosball_tables OWNER TO sa;

--
-- TOC entry 223 (class 1259 OID 21083)
-- Name: foosball_tables_id_seq; Type: SEQUENCE; Schema: public; Owner: sa
--

CREATE SEQUENCE public.foosball_tables_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.foosball_tables_id_seq OWNER TO sa;

--
-- TOC entry 3432 (class 0 OID 0)
-- Dependencies: 223
-- Name: foosball_tables_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: sa
--

ALTER SEQUENCE public.foosball_tables_id_seq OWNED BY public.foosball_tables.id;


--
-- TOC entry 230 (class 1259 OID 21140)
-- Name: match_logs; Type: TABLE; Schema: public; Owner: sa
--

CREATE TABLE public.match_logs (
    id integer NOT NULL,
    match_id integer,
    team_id integer,
    log_time timestamp without time zone,
    side character varying(10)
);


ALTER TABLE public.match_logs OWNER TO sa;

--
-- TOC entry 229 (class 1259 OID 21139)
-- Name: match_logs_id_seq; Type: SEQUENCE; Schema: public; Owner: sa
--

CREATE SEQUENCE public.match_logs_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.match_logs_id_seq OWNER TO sa;

--
-- TOC entry 3433 (class 0 OID 0)
-- Dependencies: 229
-- Name: match_logs_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: sa
--

ALTER SEQUENCE public.match_logs_id_seq OWNED BY public.match_logs.id;


--
-- TOC entry 226 (class 1259 OID 21101)
-- Name: teams; Type: TABLE; Schema: public; Owner: sa
--

CREATE TABLE public.teams (
    id integer NOT NULL,
    user1_id integer NOT NULL,
    user2_id integer
);


ALTER TABLE public.teams OWNER TO sa;

--
-- TOC entry 225 (class 1259 OID 21100)
-- Name: teams_id_seq; Type: SEQUENCE; Schema: public; Owner: sa
--

CREATE SEQUENCE public.teams_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.teams_id_seq OWNER TO sa;

--
-- TOC entry 3434 (class 0 OID 0)
-- Dependencies: 225
-- Name: teams_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: sa
--

ALTER SEQUENCE public.teams_id_seq OWNED BY public.teams.id;


--
-- TOC entry 222 (class 1259 OID 21063)
-- Name: users; Type: TABLE; Schema: public; Owner: sa
--

CREATE TABLE public.users (
    id integer NOT NULL,
    first_name character varying(100),
    last_name character varying(100),
    email character varying(100),
    password character varying(255),
    department_id integer,
    company_id integer,
    elo_1v1 integer,
    elo_2v2 integer
);


ALTER TABLE public.users OWNER TO sa;

--
-- TOC entry 221 (class 1259 OID 21062)
-- Name: users_id_seq; Type: SEQUENCE; Schema: public; Owner: sa
--

CREATE SEQUENCE public.users_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.users_id_seq OWNER TO sa;

--
-- TOC entry 3435 (class 0 OID 0)
-- Dependencies: 221
-- Name: users_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: sa
--

ALTER SEQUENCE public.users_id_seq OWNED BY public.users.id;


--
-- TOC entry 3240 (class 2604 OID 21047)
-- Name: companies id; Type: DEFAULT; Schema: public; Owner: sa
--

ALTER TABLE ONLY public.companies ALTER COLUMN id SET DEFAULT nextval('public.companies_id_seq'::regclass);


--
-- TOC entry 3241 (class 2604 OID 21054)
-- Name: departments id; Type: DEFAULT; Schema: public; Owner: sa
--

ALTER TABLE ONLY public.departments ALTER COLUMN id SET DEFAULT nextval('public.departments_id_seq'::regclass);


--
-- TOC entry 3245 (class 2604 OID 21121)
-- Name: foosball_matches id; Type: DEFAULT; Schema: public; Owner: sa
--

ALTER TABLE ONLY public.foosball_matches ALTER COLUMN id SET DEFAULT nextval('public.foosball_matches_id_seq'::regclass);


--
-- TOC entry 3243 (class 2604 OID 21087)
-- Name: foosball_tables id; Type: DEFAULT; Schema: public; Owner: sa
--

ALTER TABLE ONLY public.foosball_tables ALTER COLUMN id SET DEFAULT nextval('public.foosball_tables_id_seq'::regclass);


--
-- TOC entry 3249 (class 2604 OID 21143)
-- Name: match_logs id; Type: DEFAULT; Schema: public; Owner: sa
--

ALTER TABLE ONLY public.match_logs ALTER COLUMN id SET DEFAULT nextval('public.match_logs_id_seq'::regclass);


--
-- TOC entry 3244 (class 2604 OID 21104)
-- Name: teams id; Type: DEFAULT; Schema: public; Owner: sa
--

ALTER TABLE ONLY public.teams ALTER COLUMN id SET DEFAULT nextval('public.teams_id_seq'::regclass);


--
-- TOC entry 3242 (class 2604 OID 21066)
-- Name: users id; Type: DEFAULT; Schema: public; Owner: sa
--

ALTER TABLE ONLY public.users ALTER COLUMN id SET DEFAULT nextval('public.users_id_seq'::regclass);


--
-- TOC entry 3251 (class 2606 OID 21049)
-- Name: companies companies_pkey; Type: CONSTRAINT; Schema: public; Owner: sa
--

ALTER TABLE ONLY public.companies
    ADD CONSTRAINT companies_pkey PRIMARY KEY (id);


--
-- TOC entry 3253 (class 2606 OID 21056)
-- Name: departments departments_pkey; Type: CONSTRAINT; Schema: public; Owner: sa
--

ALTER TABLE ONLY public.departments
    ADD CONSTRAINT departments_pkey PRIMARY KEY (id);


--
-- TOC entry 3263 (class 2606 OID 21123)
-- Name: foosball_matches foosball_matches_pkey; Type: CONSTRAINT; Schema: public; Owner: sa
--

ALTER TABLE ONLY public.foosball_matches
    ADD CONSTRAINT foosball_matches_pkey PRIMARY KEY (id);


--
-- TOC entry 3259 (class 2606 OID 21089)
-- Name: foosball_tables foosball_tables_pkey; Type: CONSTRAINT; Schema: public; Owner: sa
--

ALTER TABLE ONLY public.foosball_tables
    ADD CONSTRAINT foosball_tables_pkey PRIMARY KEY (id);


--
-- TOC entry 3265 (class 2606 OID 21145)
-- Name: match_logs match_logs_pkey; Type: CONSTRAINT; Schema: public; Owner: sa
--

ALTER TABLE ONLY public.match_logs
    ADD CONSTRAINT match_logs_pkey PRIMARY KEY (id);


--
-- TOC entry 3261 (class 2606 OID 21106)
-- Name: teams teams_pkey; Type: CONSTRAINT; Schema: public; Owner: sa
--

ALTER TABLE ONLY public.teams
    ADD CONSTRAINT teams_pkey PRIMARY KEY (id);


--
-- TOC entry 3255 (class 2606 OID 21072)
-- Name: users users_email_key; Type: CONSTRAINT; Schema: public; Owner: sa
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_email_key UNIQUE (email);


--
-- TOC entry 3257 (class 2606 OID 21070)
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: sa
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- TOC entry 3266 (class 2606 OID 21057)
-- Name: departments departments_company_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: sa
--

ALTER TABLE ONLY public.departments
    ADD CONSTRAINT departments_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.companies(id);


--
-- TOC entry 3274 (class 2606 OID 21134)
-- Name: foosball_matches foosball_matches_blue_team_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: sa
--

ALTER TABLE ONLY public.foosball_matches
    ADD CONSTRAINT foosball_matches_blue_team_id_fkey FOREIGN KEY (blue_team_id) REFERENCES public.teams(id);


--
-- TOC entry 3275 (class 2606 OID 21129)
-- Name: foosball_matches foosball_matches_red_team_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: sa
--

ALTER TABLE ONLY public.foosball_matches
    ADD CONSTRAINT foosball_matches_red_team_id_fkey FOREIGN KEY (red_team_id) REFERENCES public.teams(id);


--
-- TOC entry 3276 (class 2606 OID 21124)
-- Name: foosball_matches foosball_matches_table_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: sa
--

ALTER TABLE ONLY public.foosball_matches
    ADD CONSTRAINT foosball_matches_table_id_fkey FOREIGN KEY (table_id) REFERENCES public.foosball_tables(id);


--
-- TOC entry 3269 (class 2606 OID 29027)
-- Name: foosball_tables foosball_tables_active_match_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: sa
--

ALTER TABLE ONLY public.foosball_tables
    ADD CONSTRAINT foosball_tables_active_match_id_fkey FOREIGN KEY (active_match_id) REFERENCES public.foosball_matches(id) ON DELETE SET NULL;


--
-- TOC entry 3270 (class 2606 OID 21095)
-- Name: foosball_tables foosball_tables_company_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: sa
--

ALTER TABLE ONLY public.foosball_tables
    ADD CONSTRAINT foosball_tables_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.companies(id);


--
-- TOC entry 3271 (class 2606 OID 21090)
-- Name: foosball_tables foosball_tables_department_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: sa
--

ALTER TABLE ONLY public.foosball_tables
    ADD CONSTRAINT foosball_tables_department_id_fkey FOREIGN KEY (department_id) REFERENCES public.departments(id);


--
-- TOC entry 3277 (class 2606 OID 21146)
-- Name: match_logs match_logs_match_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: sa
--

ALTER TABLE ONLY public.match_logs
    ADD CONSTRAINT match_logs_match_id_fkey FOREIGN KEY (match_id) REFERENCES public.foosball_matches(id);


--
-- TOC entry 3278 (class 2606 OID 21151)
-- Name: match_logs match_logs_team_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: sa
--

ALTER TABLE ONLY public.match_logs
    ADD CONSTRAINT match_logs_team_id_fkey FOREIGN KEY (team_id) REFERENCES public.teams(id);


--
-- TOC entry 3272 (class 2606 OID 21107)
-- Name: teams teams_user1_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: sa
--

ALTER TABLE ONLY public.teams
    ADD CONSTRAINT teams_user1_id_fkey FOREIGN KEY (user1_id) REFERENCES public.users(id);


--
-- TOC entry 3273 (class 2606 OID 21112)
-- Name: teams teams_user2_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: sa
--

ALTER TABLE ONLY public.teams
    ADD CONSTRAINT teams_user2_id_fkey FOREIGN KEY (user2_id) REFERENCES public.users(id);


--
-- TOC entry 3267 (class 2606 OID 21078)
-- Name: users users_company_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: sa
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.companies(id);


--
-- TOC entry 3268 (class 2606 OID 21073)
-- Name: users users_department_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: sa
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_department_id_fkey FOREIGN KEY (department_id) REFERENCES public.departments(id);


-- Completed on 2024-12-05 13:57:37

--
-- PostgreSQL database dump complete
--

