--
-- PostgreSQL database dump
--

\restrict 0W7krHvs2wdC5fLvfa0PUQVUWvZFpE0P9o5o5ZJAcneiMwOoloGYQ4DRezm0zSL

-- Dumped from database version 15.18 (Debian 15.18-1.pgdg12+1)
-- Dumped by pg_dump version 15.18 (Debian 15.18-1.pgdg12+1)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: vector; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS vector WITH SCHEMA public;


--
-- Name: EXTENSION vector; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION vector IS 'vector data type and ivfflat and hnsw access methods';


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


ALTER TABLE public."__EFMigrationsHistory" OWNER TO postgres;

--
-- Name: accounts; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.accounts (
    account_id integer NOT NULL,
    email character varying(255) NOT NULL,
    username character varying(255),
    password_hash text,
    phone_number character varying(50),
    account_status character varying(50),
    account_flag_count integer DEFAULT 0,
    auth_provider character varying(50),
    avatar_url text,
    refresh_token text,
    refresh_token_expiry_time timestamp without time zone,
    is_verified boolean DEFAULT false NOT NULL,
    account_created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    account_updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    account_last_login_at timestamp without time zone
);


ALTER TABLE public.accounts OWNER TO postgres;

--
-- Name: accounts_account_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.accounts_account_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.accounts_account_id_seq OWNER TO postgres;

--
-- Name: accounts_account_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.accounts_account_id_seq OWNED BY public.accounts.account_id;


--
-- Name: ai_models; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ai_models (
    model_id integer NOT NULL,
    model_name character varying(255) NOT NULL,
    model_type character varying(50),
    model_provider character varying(50),
    model_version character varying(50),
    model_status character varying(50),
    description text,
    model_created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    model_updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    model_path character varying(255),
    process_type character varying(255)
);


ALTER TABLE public.ai_models OWNER TO postgres;

--
-- Name: ai_models_model_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.ai_models_model_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.ai_models_model_id_seq OWNER TO postgres;

--
-- Name: ai_models_model_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.ai_models_model_id_seq OWNED BY public.ai_models.model_id;


--
-- Name: audit_logs; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.audit_logs (
    log_id integer NOT NULL,
    actor_id integer,
    action_type character varying(100) NOT NULL,
    target_type character varying(100),
    target_id integer,
    details text,
    ip_address character varying(45),
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.audit_logs OWNER TO postgres;

--
-- Name: audit_logs_log_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.audit_logs_log_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.audit_logs_log_id_seq OWNER TO postgres;

--
-- Name: audit_logs_log_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.audit_logs_log_id_seq OWNED BY public.audit_logs.log_id;


--
-- Name: avatar_frames; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.avatar_frames (
    id integer NOT NULL,
    name character varying(100) NOT NULL,
    image_url text NOT NULL,
    description text,
    requirement_type character varying(50),
    requirement_value integer DEFAULT 0,
    is_active boolean DEFAULT true,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.avatar_frames OWNER TO postgres;

--
-- Name: avatar_frames_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.avatar_frames_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.avatar_frames_id_seq OWNER TO postgres;

--
-- Name: avatar_frames_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.avatar_frames_id_seq OWNED BY public.avatar_frames.id;


--
-- Name: cart_items; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cart_items (
    id integer NOT NULL,
    user_id integer,
    course_id integer,
    added_date timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    price numeric(10,2)
);


ALTER TABLE public.cart_items OWNER TO postgres;

--
-- Name: cart_items_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.cart_items_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.cart_items_id_seq OWNER TO postgres;

--
-- Name: cart_items_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.cart_items_id_seq OWNED BY public.cart_items.id;


--
-- Name: categories; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.categories (
    category_id integer NOT NULL,
    categories_name character varying(255) NOT NULL,
    description text,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    category_status character varying(50)
);


ALTER TABLE public.categories OWNER TO postgres;

--
-- Name: categories_category_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.categories_category_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.categories_category_id_seq OWNER TO postgres;

--
-- Name: categories_category_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.categories_category_id_seq OWNED BY public.categories.category_id;


--
-- Name: chat_participants; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.chat_participants (
    chat_id integer NOT NULL,
    account_id integer NOT NULL,
    role character varying(50) DEFAULT 'member'::character varying,
    unread_count integer DEFAULT 0,
    last_read_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    joined_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    cleared_at timestamp without time zone
);


ALTER TABLE public.chat_participants OWNER TO postgres;

--
-- Name: chats; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.chats (
    chat_id integer NOT NULL,
    chat_name character varying(255),
    chat_type character varying(50) DEFAULT 'private'::character varying,
    context_type character varying(50),
    context_id integer,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    last_message_at timestamp without time zone
);


ALTER TABLE public.chats OWNER TO postgres;

--
-- Name: chats_chat_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.chats_chat_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.chats_chat_id_seq OWNER TO postgres;

--
-- Name: chats_chat_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.chats_chat_id_seq OWNED BY public.chats.chat_id;


--
-- Name: coupons; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.coupons (
    coupon_id integer NOT NULL,
    manager_id integer,
    coupon_code character varying(50) NOT NULL,
    coupon_type character varying(50),
    discount_value numeric(10,2) NOT NULL,
    min_order_value numeric(10,2) DEFAULT 0 NOT NULL,
    start_date timestamp without time zone,
    end_date timestamp without time zone,
    usage_limit integer,
    used_count integer DEFAULT 0,
    is_active boolean DEFAULT true
);


ALTER TABLE public.coupons OWNER TO postgres;

--
-- Name: coupons_coupon_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.coupons_coupon_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.coupons_coupon_id_seq OWNER TO postgres;

--
-- Name: coupons_coupon_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.coupons_coupon_id_seq OWNED BY public.coupons.coupon_id;


--
-- Name: course_ai_usage_logs; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.course_ai_usage_logs (
    log_id integer NOT NULL,
    integration_id integer,
    interaction_type character varying(50),
    input_json jsonb,
    output_json jsonb,
    latency_ms real,
    token_usage real,
    error_message text,
    log_created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.course_ai_usage_logs OWNER TO postgres;

--
-- Name: course_ai_usage_logs_log_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.course_ai_usage_logs_log_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.course_ai_usage_logs_log_id_seq OWNER TO postgres;

--
-- Name: course_ai_usage_logs_log_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.course_ai_usage_logs_log_id_seq OWNED BY public.course_ai_usage_logs.log_id;


--
-- Name: course_exts; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.course_exts (
    course_id integer NOT NULL,
    title_hash character(32),
    description_hash character(32),
    what_you_will_learn_hash character(32),
    requirements_hash character(32),
    thumbnail_hash character(32)
);


ALTER TABLE public.course_exts OWNER TO postgres;

--
-- Name: course_reports; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.course_reports (
    course_report_id integer NOT NULL,
    reporter_id integer,
    course_id integer,
    resolver_id integer,
    reason character varying(255),
    description text,
    course_reports_status character varying(50),
    resolution_note text,
    resolved_at timestamp without time zone,
    access_granted_until timestamp without time zone,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.course_reports OWNER TO postgres;

--
-- Name: course_reports_course_report_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.course_reports_course_report_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.course_reports_course_report_id_seq OWNER TO postgres;

--
-- Name: course_reports_course_report_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.course_reports_course_report_id_seq OWNED BY public.course_reports.course_report_id;


--
-- Name: course_review_moderation_logs; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.course_review_moderation_logs (
    log_id integer NOT NULL,
    model_id integer,
    course_review_id integer,
    input_json jsonb,
    output_json jsonb,
    latency_ms real,
    error_message text,
    log_created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.course_review_moderation_logs OWNER TO postgres;

--
-- Name: course_review_moderation_logs_log_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.course_review_moderation_logs_log_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.course_review_moderation_logs_log_id_seq OWNER TO postgres;

--
-- Name: course_review_moderation_logs_log_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.course_review_moderation_logs_log_id_seq OWNED BY public.course_review_moderation_logs.log_id;


--
-- Name: course_review_reports; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.course_review_reports (
    course_review_report_id integer NOT NULL,
    reporter_id integer,
    course_review_id integer,
    resolver_id integer,
    reason character varying(255),
    description text,
    user_reports_status character varying(50),
    resolution_note text,
    resolved_at timestamp without time zone,
    access_granted_until timestamp without time zone,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.course_review_reports OWNER TO postgres;

--
-- Name: course_review_reports_course_review_report_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.course_review_reports_course_review_report_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.course_review_reports_course_review_report_id_seq OWNER TO postgres;

--
-- Name: course_review_reports_course_review_report_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.course_review_reports_course_review_report_id_seq OWNED BY public.course_review_reports.course_review_report_id;


--
-- Name: course_reviews; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.course_reviews (
    course_review_id integer NOT NULL,
    enrollment_id integer NOT NULL,
    rating numeric(3,2),
    comment text,
    course_review_status text DEFAULT 'ok'::text NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    is_removed boolean DEFAULT false,
    CONSTRAINT course_reviews_rating_check CHECK (((rating >= (0)::numeric) AND (rating <= (5)::numeric)))
);


ALTER TABLE public.course_reviews OWNER TO postgres;

--
-- Name: course_reviews_course_review_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.course_reviews_course_review_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.course_reviews_course_review_id_seq OWNER TO postgres;

--
-- Name: course_reviews_course_review_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.course_reviews_course_review_id_seq OWNED BY public.course_reviews.course_review_id;


--
-- Name: courses; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.courses (
    course_id integer NOT NULL,
    instructor_id integer,
    category_id integer,
    coupon_id integer,
    title character varying(255) NOT NULL,
    description text,
    price numeric(10,2) NOT NULL,
    course_thumbnail_url text,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    course_status character varying(50),
    course_flag_count integer DEFAULT 0,
    what_you_will_learn text,
    requirements text,
    moderation_feedback text,
    last_approved_at timestamp without time zone,
    is_removed boolean DEFAULT false,
    threat_level integer DEFAULT 1
);


ALTER TABLE public.courses OWNER TO postgres;

--
-- Name: courses_ai_integrations; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.courses_ai_integrations (
    id integer NOT NULL,
    model_id integer,
    course_id integer,
    role character varying(50),
    is_enabled boolean DEFAULT true NOT NULL,
    config_json jsonb,
    assigned_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.courses_ai_integrations OWNER TO postgres;

--
-- Name: courses_ai_integrations_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.courses_ai_integrations_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.courses_ai_integrations_id_seq OWNER TO postgres;

--
-- Name: courses_ai_integrations_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.courses_ai_integrations_id_seq OWNED BY public.courses_ai_integrations.id;


--
-- Name: courses_course_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.courses_course_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.courses_course_id_seq OWNER TO postgres;

--
-- Name: courses_course_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.courses_course_id_seq OWNED BY public.courses.course_id;


--
-- Name: enrollments; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.enrollments (
    enrollment_id integer NOT NULL,
    user_id integer,
    course_id integer,
    title character varying(255),
    description text,
    completed_date date,
    is_completed boolean DEFAULT false,
    enroll_date date DEFAULT CURRENT_DATE,
    last_accessed_at timestamp without time zone,
    enrollment_status character varying(50)
);


ALTER TABLE public.enrollments OWNER TO postgres;

--
-- Name: enrollments_enrollment_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.enrollments_enrollment_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.enrollments_enrollment_id_seq OWNER TO postgres;

--
-- Name: enrollments_enrollment_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.enrollments_enrollment_id_seq OWNED BY public.enrollments.enrollment_id;


--
-- Name: gifts; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.gifts (
    gift_id integer NOT NULL,
    order_item_id integer NOT NULL,
    sender_id integer,
    recipient_email character varying(255) NOT NULL,
    recipient_name character varying(255),
    gift_message text,
    card_theme character varying(50) DEFAULT 'classic'::character varying,
    redemption_token character varying(255) NOT NULL,
    is_claimed boolean DEFAULT false,
    claimed_by_user_id integer,
    claimed_at timestamp without time zone,
    delivery_status character varying(50) DEFAULT 'pending'::character varying,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.gifts OWNER TO postgres;

--
-- Name: gifts_gift_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.gifts_gift_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.gifts_gift_id_seq OWNER TO postgres;

--
-- Name: gifts_gift_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.gifts_gift_id_seq OWNED BY public.gifts.gift_id;


--
-- Name: instructor_payouts; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.instructor_payouts (
    payout_id integer NOT NULL,
    transaction_id integer,
    instructor_id integer,
    payout_amount numeric(10,2) NOT NULL,
    payout_date timestamp without time zone NOT NULL,
    is_paid boolean DEFAULT false NOT NULL,
    payout_status character varying(20) DEFAULT 'pending'::character varying NOT NULL,
    stripe_transfer_id character varying(255),
    stripe_payout_id character varying(255),
    paid_to_bank_at timestamp without time zone,
    CONSTRAINT instructor_payouts_payout_status_check CHECK (((payout_status)::text = ANY ((ARRAY['pending'::character varying, 'transferred'::character varying, 'in_transit'::character varying, 'paid'::character varying, 'failed'::character varying, 'refunded'::character varying])::text[])))
);


ALTER TABLE public.instructor_payouts OWNER TO postgres;

--
-- Name: instructor_payouts_payout_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.instructor_payouts_payout_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.instructor_payouts_payout_id_seq OWNER TO postgres;

--
-- Name: instructor_payouts_payout_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.instructor_payouts_payout_id_seq OWNED BY public.instructor_payouts.payout_id;


--
-- Name: instructors; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.instructors (
    instructor_id integer NOT NULL,
    stripe_account_id character varying(255),
    stripe_onboarding_status character varying(50),
    payouts_enabled boolean DEFAULT false,
    charges_enabled boolean DEFAULT false,
    professional_title character varying(255),
    expertise_categories character varying(255),
    linkedin_url text,
    youtube_url text,
    facebook_url text,
    document_url text,
    approval_status character varying(50) DEFAULT 'Pending'::character varying,
    stripe_country character varying(2),
    rejection_reason text
);


ALTER TABLE public.instructors OWNER TO postgres;

--
-- Name: learning_materials; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.learning_materials (
    material_id integer NOT NULL,
    lesson_id integer,
    title character varying(255) NOT NULL,
    description text,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    learning_status character varying(50),
    moderation_feedback text,
    material_url text,
    material_metadata jsonb,
    cloud_public_id text
);


ALTER TABLE public.learning_materials OWNER TO postgres;

--
-- Name: learning_materials_material_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.learning_materials_material_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.learning_materials_material_id_seq OWNER TO postgres;

--
-- Name: learning_materials_material_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.learning_materials_material_id_seq OWNED BY public.learning_materials.material_id;


--
-- Name: lesson_review_moderation_logs; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.lesson_review_moderation_logs (
    log_id integer NOT NULL,
    model_id integer,
    lesson_review_id integer,
    input_json jsonb,
    output_json jsonb,
    latency_ms real,
    error_message text,
    log_created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.lesson_review_moderation_logs OWNER TO postgres;

--
-- Name: lesson_review_moderation_logs_log_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.lesson_review_moderation_logs_log_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.lesson_review_moderation_logs_log_id_seq OWNER TO postgres;

--
-- Name: lesson_review_moderation_logs_log_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.lesson_review_moderation_logs_log_id_seq OWNED BY public.lesson_review_moderation_logs.log_id;


--
-- Name: lesson_review_reports; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.lesson_review_reports (
    lesson_review_report_id integer NOT NULL,
    reporter_id integer,
    lesson_review_id integer,
    resolver_id integer,
    reason character varying(255),
    description text,
    user_reports_status character varying(50),
    resolution_note text,
    resolved_at timestamp without time zone,
    access_granted_until timestamp without time zone,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.lesson_review_reports OWNER TO postgres;

--
-- Name: lesson_review_reports_lesson_review_report_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.lesson_review_reports_lesson_review_report_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.lesson_review_reports_lesson_review_report_id_seq OWNER TO postgres;

--
-- Name: lesson_review_reports_lesson_review_report_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.lesson_review_reports_lesson_review_report_id_seq OWNED BY public.lesson_review_reports.lesson_review_report_id;


--
-- Name: lesson_reviews; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.lesson_reviews (
    lesson_review_id integer NOT NULL,
    enrollment_id integer NOT NULL,
    lesson_id integer,
    rating numeric(3,2),
    comment text,
    lesson_review_status text DEFAULT 'ok'::text NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    is_removed boolean DEFAULT false,
    CONSTRAINT lesson_reviews_rating_check CHECK (((rating >= (0)::numeric) AND (rating <= (5)::numeric)))
);


ALTER TABLE public.lesson_reviews OWNER TO postgres;

--
-- Name: lesson_reviews_lesson_review_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.lesson_reviews_lesson_review_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.lesson_reviews_lesson_review_id_seq OWNER TO postgres;

--
-- Name: lesson_reviews_lesson_review_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.lesson_reviews_lesson_review_id_seq OWNED BY public.lesson_reviews.lesson_review_id;


--
-- Name: lessons; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.lessons (
    lesson_id integer NOT NULL,
    course_id integer,
    title character varying(255) NOT NULL,
    description text,
    thumbnail_url text,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    lesson_status character varying(50),
    is_removed boolean DEFAULT false
);


ALTER TABLE public.lessons OWNER TO postgres;

--
-- Name: lessons_lesson_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.lessons_lesson_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.lessons_lesson_id_seq OWNER TO postgres;

--
-- Name: lessons_lesson_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.lessons_lesson_id_seq OWNED BY public.lessons.lesson_id;


--
-- Name: lockouts; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.lockouts (
    lockout_id integer NOT NULL,
    account_id integer,
    lockout_type character varying(50),
    lockout_level character varying(50),
    lockout_start timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    lockout_end timestamp without time zone
);


ALTER TABLE public.lockouts OWNER TO postgres;

--
-- Name: lockouts_lockout_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.lockouts_lockout_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.lockouts_lockout_id_seq OWNER TO postgres;

--
-- Name: lockouts_lockout_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.lockouts_lockout_id_seq OWNED BY public.lockouts.lockout_id;


--
-- Name: managers; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.managers (
    manager_id integer NOT NULL,
    role character varying(50),
    display_name character varying(255) NOT NULL,
    full_name character varying(255),
    phone_number character varying(50),
    avatar_url text,
    bio text
);


ALTER TABLE public.managers OWNER TO postgres;

--
-- Name: material_completions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.material_completions (
    id integer NOT NULL,
    enrollment_id integer NOT NULL,
    material_id integer NOT NULL,
    completed_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.material_completions OWNER TO postgres;

--
-- Name: material_completions_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.material_completions_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.material_completions_id_seq OWNER TO postgres;

--
-- Name: material_completions_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.material_completions_id_seq OWNED BY public.material_completions.id;


--
-- Name: media_embeddings; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.media_embeddings (
    media_embedding_id integer NOT NULL,
    material_id integer,
    media_embedding public.vector(512),
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.media_embeddings OWNER TO postgres;

--
-- Name: media_embeddings_media_embedding_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.media_embeddings_media_embedding_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.media_embeddings_media_embedding_id_seq OWNER TO postgres;

--
-- Name: media_embeddings_media_embedding_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.media_embeddings_media_embedding_id_seq OWNED BY public.media_embeddings.media_embedding_id;


--
-- Name: message_attachments; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.message_attachments (
    attachment_id integer NOT NULL,
    message_id integer,
    file_url text NOT NULL,
    file_name character varying(255),
    file_type character varying(50),
    file_size bigint,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.message_attachments OWNER TO postgres;

--
-- Name: message_attachments_attachment_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.message_attachments_attachment_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.message_attachments_attachment_id_seq OWNER TO postgres;

--
-- Name: message_attachments_attachment_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.message_attachments_attachment_id_seq OWNED BY public.message_attachments.attachment_id;


--
-- Name: message_moderation_logs; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.message_moderation_logs (
    log_id integer NOT NULL,
    model_id integer,
    message_id integer,
    input_json jsonb,
    output_json jsonb,
    latency_ms real,
    error_message text,
    log_created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.message_moderation_logs OWNER TO postgres;

--
-- Name: message_moderation_logs_log_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.message_moderation_logs_log_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.message_moderation_logs_log_id_seq OWNER TO postgres;

--
-- Name: message_moderation_logs_log_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.message_moderation_logs_log_id_seq OWNED BY public.message_moderation_logs.log_id;


--
-- Name: messages; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.messages (
    message_id integer NOT NULL,
    chat_id integer,
    sender_id integer,
    content text NOT NULL,
    is_seen boolean DEFAULT false,
    message_status character varying(50) DEFAULT 'ok'::character varying,
    sent_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    received_at timestamp without time zone
);


ALTER TABLE public.messages OWNER TO postgres;

--
-- Name: messages_message_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.messages_message_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.messages_message_id_seq OWNER TO postgres;

--
-- Name: messages_message_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.messages_message_id_seq OWNED BY public.messages.message_id;


--
-- Name: notifications; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.notifications (
    notification_id integer NOT NULL,
    sender_id integer,
    receiver_id integer,
    title character varying(255),
    content text,
    link_action text,
    is_read boolean DEFAULT false,
    is_removed boolean DEFAULT false,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.notifications OWNER TO postgres;

--
-- Name: notifications_notification_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.notifications_notification_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.notifications_notification_id_seq OWNER TO postgres;

--
-- Name: notifications_notification_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.notifications_notification_id_seq OWNED BY public.notifications.notification_id;


--
-- Name: order_info; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.order_info (
    order_id integer NOT NULL,
    user_id integer,
    order_date timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    order_status character varying(50),
    payment_method character varying(50)
);


ALTER TABLE public.order_info OWNER TO postgres;

--
-- Name: order_info_order_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.order_info_order_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.order_info_order_id_seq OWNER TO postgres;

--
-- Name: order_info_order_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.order_info_order_id_seq OWNED BY public.order_info.order_id;


--
-- Name: order_items; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.order_items (
    id integer NOT NULL,
    order_id integer,
    course_id integer,
    purchase_price numeric(10,2) NOT NULL,
    coupon_used boolean DEFAULT false,
    original_price numeric(10,2),
    coupon_code character varying(50),
    coupon_type character varying(50),
    discount_amount numeric(10,2) DEFAULT 0
);


ALTER TABLE public.order_items OWNER TO postgres;

--
-- Name: order_items_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.order_items_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.order_items_id_seq OWNER TO postgres;

--
-- Name: order_items_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.order_items_id_seq OWNED BY public.order_items.id;


--
-- Name: platform_withdrawals; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.platform_withdrawals (
    withdrawal_id integer NOT NULL,
    manager_id integer,
    amount numeric(10,2) NOT NULL,
    currency character varying(10) DEFAULT 'usd'::character varying,
    stripe_payout_id character varying(255),
    status character varying(20) DEFAULT 'pending'::character varying NOT NULL,
    description text,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    arrived_at timestamp without time zone,
    CONSTRAINT platform_withdrawals_status_check CHECK (((status)::text = ANY ((ARRAY['pending'::character varying, 'in_transit'::character varying, 'paid'::character varying, 'failed'::character varying, 'canceled'::character varying])::text[])))
);


ALTER TABLE public.platform_withdrawals OWNER TO postgres;

--
-- Name: platform_withdrawals_withdrawal_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.platform_withdrawals_withdrawal_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.platform_withdrawals_withdrawal_id_seq OWNER TO postgres;

--
-- Name: platform_withdrawals_withdrawal_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.platform_withdrawals_withdrawal_id_seq OWNED BY public.platform_withdrawals.withdrawal_id;


--
-- Name: system_configs; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.system_configs (
    config_id integer NOT NULL,
    manager_id integer,
    config_key character varying(255) NOT NULL,
    config_value text,
    description text,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.system_configs OWNER TO postgres;

--
-- Name: system_configs_config_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.system_configs_config_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.system_configs_config_id_seq OWNER TO postgres;

--
-- Name: system_configs_config_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.system_configs_config_id_seq OWNED BY public.system_configs.config_id;


--
-- Name: text_embeddings; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.text_embeddings (
    text_embedding_id integer NOT NULL,
    material_id integer,
    text_embedding public.vector(768),
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.text_embeddings OWNER TO postgres;

--
-- Name: text_embeddings_text_embedding_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.text_embeddings_text_embedding_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.text_embeddings_text_embedding_id_seq OWNER TO postgres;

--
-- Name: text_embeddings_text_embedding_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.text_embeddings_text_embedding_id_seq OWNED BY public.text_embeddings.text_embedding_id;


--
-- Name: transaction_exts; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.transaction_exts (
    transaction_id integer NOT NULL,
    refund_reason text,
    refund_admin_note text,
    refund_requested_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.transaction_exts OWNER TO postgres;

--
-- Name: transactions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.transactions (
    transaction_id integer NOT NULL,
    order_item_id integer,
    account_from integer,
    account_to integer,
    amount numeric(10,2) NOT NULL,
    transfer_rate numeric(5,2) DEFAULT 100.00 NOT NULL,
    stripe_session_id character varying(255),
    stripe_paymentintent_id character varying(255),
    currency character varying(10) DEFAULT 'VND'::character varying,
    transactions_status character varying(50),
    transaction_type character varying(50),
    transaction_created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.transactions OWNER TO postgres;

--
-- Name: transactions_transaction_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.transactions_transaction_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.transactions_transaction_id_seq OWNER TO postgres;

--
-- Name: transactions_transaction_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.transactions_transaction_id_seq OWNED BY public.transactions.transaction_id;


--
-- Name: user_avatar_frames; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.user_avatar_frames (
    user_id integer NOT NULL,
    frame_id integer NOT NULL,
    unlocked_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    is_equipped boolean DEFAULT false
);


ALTER TABLE public.user_avatar_frames OWNER TO postgres;

--
-- Name: users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.users (
    user_id integer NOT NULL,
    full_name character varying(255) NOT NULL,
    bio text,
    date_of_birth date
);


ALTER TABLE public.users OWNER TO postgres;

--
-- Name: view_lesson_stats; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW public.view_lesson_stats AS
 SELECT l.lesson_id,
    l.course_id,
    count(lm.material_id) AS material_count,
    COALESCE(sum(((lm.material_metadata ->> 'duration'::text))::integer), (0)::bigint) AS lesson_duration
   FROM (public.lessons l
     LEFT JOIN public.learning_materials lm ON ((lm.lesson_id = l.lesson_id)))
  GROUP BY l.lesson_id, l.course_id;


ALTER TABLE public.view_lesson_stats OWNER TO postgres;

--
-- Name: view_course_stats; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW public.view_course_stats AS
 SELECT c.course_id,
    COALESCE(avg(cr.rating), (0)::numeric) AS rating_average,
    count(DISTINCT e.enrollment_id) AS total_students,
    count(DISTINCT cr.course_review_id) AS total_reviews,
    COALESCE(cs.total_lessons, (0)::bigint) AS total_lessons,
    COALESCE(cs.total_materials, (0)::numeric) AS total_materials,
    COALESCE(cs.total_duration, (0)::numeric) AS total_duration
   FROM (((public.courses c
     LEFT JOIN public.enrollments e ON ((e.course_id = c.course_id)))
     LEFT JOIN public.course_reviews cr ON (((cr.enrollment_id = e.enrollment_id) AND (cr.is_removed = false))))
     LEFT JOIN ( SELECT view_lesson_stats.course_id,
            count(view_lesson_stats.lesson_id) AS total_lessons,
            sum(view_lesson_stats.material_count) AS total_materials,
            sum(view_lesson_stats.lesson_duration) AS total_duration
           FROM public.view_lesson_stats
          GROUP BY view_lesson_stats.course_id) cs ON ((cs.course_id = c.course_id)))
  GROUP BY c.course_id, cs.total_lessons, cs.total_materials, cs.total_duration;


ALTER TABLE public.view_course_stats OWNER TO postgres;

--
-- Name: view_instructor_stats; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW public.view_instructor_stats AS
 SELECT i.instructor_id,
    COALESCE(avg(cr.rating), (0)::numeric) AS instructor_rating,
    COALESCE(sum(ip.payout_amount), (0)::numeric) AS total_revenue,
    count(DISTINCT e.enrollment_id) AS total_students_count
   FROM ((((public.instructors i
     LEFT JOIN public.courses c ON ((c.instructor_id = i.instructor_id)))
     LEFT JOIN public.enrollments e ON ((e.course_id = c.course_id)))
     LEFT JOIN public.course_reviews cr ON (((cr.enrollment_id = e.enrollment_id) AND (cr.is_removed = false))))
     LEFT JOIN public.instructor_payouts ip ON ((ip.instructor_id = i.instructor_id)))
  GROUP BY i.instructor_id;


ALTER TABLE public.view_instructor_stats OWNER TO postgres;

--
-- Name: view_order_stats; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW public.view_order_stats AS
 SELECT o.order_id,
    o.user_id,
    COALESCE(sum(oi.purchase_price), (0)::numeric) AS total_amount
   FROM (public.order_info o
     LEFT JOIN public.order_items oi ON ((oi.order_id = o.order_id)))
  GROUP BY o.order_id, o.user_id;


ALTER TABLE public.view_order_stats OWNER TO postgres;

--
-- Name: view_user_stats; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW public.view_user_stats AS
 SELECT u.user_id,
    count(DISTINCT e.enrollment_id) AS enrolled_courses_count,
    COALESCE(sum(oi.purchase_price), (0)::numeric) AS total_spent
   FROM (((public.users u
     LEFT JOIN public.enrollments e ON ((e.user_id = u.user_id)))
     LEFT JOIN public.order_info o ON (((o.user_id = u.user_id) AND ((o.order_status)::text = 'paid'::text))))
     LEFT JOIN public.order_items oi ON ((oi.order_id = o.order_id)))
  GROUP BY u.user_id;


ALTER TABLE public.view_user_stats OWNER TO postgres;

--
-- Name: wishlist_items; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.wishlist_items (
    id integer NOT NULL,
    user_id integer,
    course_id integer,
    added_date timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.wishlist_items OWNER TO postgres;

--
-- Name: wishlist_items_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.wishlist_items_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.wishlist_items_id_seq OWNER TO postgres;

--
-- Name: wishlist_items_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.wishlist_items_id_seq OWNED BY public.wishlist_items.id;


--
-- Name: accounts account_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.accounts ALTER COLUMN account_id SET DEFAULT nextval('public.accounts_account_id_seq'::regclass);


--
-- Name: ai_models model_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ai_models ALTER COLUMN model_id SET DEFAULT nextval('public.ai_models_model_id_seq'::regclass);


--
-- Name: audit_logs log_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.audit_logs ALTER COLUMN log_id SET DEFAULT nextval('public.audit_logs_log_id_seq'::regclass);


--
-- Name: avatar_frames id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.avatar_frames ALTER COLUMN id SET DEFAULT nextval('public.avatar_frames_id_seq'::regclass);


--
-- Name: cart_items id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cart_items ALTER COLUMN id SET DEFAULT nextval('public.cart_items_id_seq'::regclass);


--
-- Name: categories category_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.categories ALTER COLUMN category_id SET DEFAULT nextval('public.categories_category_id_seq'::regclass);


--
-- Name: chats chat_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.chats ALTER COLUMN chat_id SET DEFAULT nextval('public.chats_chat_id_seq'::regclass);


--
-- Name: coupons coupon_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.coupons ALTER COLUMN coupon_id SET DEFAULT nextval('public.coupons_coupon_id_seq'::regclass);


--
-- Name: course_ai_usage_logs log_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.course_ai_usage_logs ALTER COLUMN log_id SET DEFAULT nextval('public.course_ai_usage_logs_log_id_seq'::regclass);


--
-- Name: course_reports course_report_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.course_reports ALTER COLUMN course_report_id SET DEFAULT nextval('public.course_reports_course_report_id_seq'::regclass);


--
-- Name: course_review_moderation_logs log_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.course_review_moderation_logs ALTER COLUMN log_id SET DEFAULT nextval('public.course_review_moderation_logs_log_id_seq'::regclass);


--
-- Name: course_review_reports course_review_report_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.course_review_reports ALTER COLUMN course_review_report_id SET DEFAULT nextval('public.course_review_reports_course_review_report_id_seq'::regclass);


--
-- Name: course_reviews course_review_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.course_reviews ALTER COLUMN course_review_id SET DEFAULT nextval('public.course_reviews_course_review_id_seq'::regclass);


--
-- Name: courses course_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.courses ALTER COLUMN course_id SET DEFAULT nextval('public.courses_course_id_seq'::regclass);


--
-- Name: courses_ai_integrations id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.courses_ai_integrations ALTER COLUMN id SET DEFAULT nextval('public.courses_ai_integrations_id_seq'::regclass);


--
-- Name: enrollments enrollment_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.enrollments ALTER COLUMN enrollment_id SET DEFAULT nextval('public.enrollments_enrollment_id_seq'::regclass);


--
-- Name: gifts gift_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.gifts ALTER COLUMN gift_id SET DEFAULT nextval('public.gifts_gift_id_seq'::regclass);


--
-- Name: instructor_payouts payout_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.instructor_payouts ALTER COLUMN payout_id SET DEFAULT nextval('public.instructor_payouts_payout_id_seq'::regclass);


--
-- Name: learning_materials material_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.learning_materials ALTER COLUMN material_id SET DEFAULT nextval('public.learning_materials_material_id_seq'::regclass);


--
-- Name: lesson_review_moderation_logs log_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.lesson_review_moderation_logs ALTER COLUMN log_id SET DEFAULT nextval('public.lesson_review_moderation_logs_log_id_seq'::regclass);


--
-- Name: lesson_review_reports lesson_review_report_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.lesson_review_reports ALTER COLUMN lesson_review_report_id SET DEFAULT nextval('public.lesson_review_reports_lesson_review_report_id_seq'::regclass);


--
-- Name: lesson_reviews lesson_review_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.lesson_reviews ALTER COLUMN lesson_review_id SET DEFAULT nextval('public.lesson_reviews_lesson_review_id_seq'::regclass);


--
-- Name: lessons lesson_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.lessons ALTER COLUMN lesson_id SET DEFAULT nextval('public.lessons_lesson_id_seq'::regclass);


--
-- Name: lockouts lockout_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.lockouts ALTER COLUMN lockout_id SET DEFAULT nextval('public.lockouts_lockout_id_seq'::regclass);


--
-- Name: material_completions id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.material_completions ALTER COLUMN id SET DEFAULT nextval('public.material_completions_id_seq'::regclass);


--
-- Name: media_embeddings media_embedding_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.media_embeddings ALTER COLUMN media_embedding_id SET DEFAULT nextval('public.media_embeddings_media_embedding_id_seq'::regclass);


--
-- Name: message_attachments attachment_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.message_attachments ALTER COLUMN attachment_id SET DEFAULT nextval('public.message_attachments_attachment_id_seq'::regclass);


--
-- Name: message_moderation_logs log_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.message_moderation_logs ALTER COLUMN log_id SET DEFAULT nextval('public.message_moderation_logs_log_id_seq'::regclass);


--
-- Name: messages message_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.messages ALTER COLUMN message_id SET DEFAULT nextval('public.messages_message_id_seq'::regclass);


--
-- Name: notifications notification_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.notifications ALTER COLUMN notification_id SET DEFAULT nextval('public.notifications_notification_id_seq'::regclass);


--
-- Name: order_info order_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.order_info ALTER COLUMN order_id SET DEFAULT nextval('public.order_info_order_id_seq'::regclass);


--
-- Name: order_items id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.order_items ALTER COLUMN id SET DEFAULT nextval('public.order_items_id_seq'::regclass);


--
-- Name: platform_withdrawals withdrawal_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.platform_withdrawals ALTER COLUMN withdrawal_id SET DEFAULT nextval('public.platform_withdrawals_withdrawal_id_seq'::regclass);


--
-- Name: system_configs config_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.system_configs ALTER COLUMN config_id SET DEFAULT nextval('public.system_configs_config_id_seq'::regclass);


--
-- Name: text_embeddings text_embedding_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.text_embeddings ALTER COLUMN text_embedding_id SET DEFAULT nextval('public.text_embeddings_text_embedding_id_seq'::regclass);


--
-- Name: transactions transaction_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.transactions ALTER COLUMN transaction_id SET DEFAULT nextval('public.transactions_transaction_id_seq'::regclass);


--
-- Name: wishlist_items id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.wishlist_items ALTER COLUMN id SET DEFAULT nextval('public.wishlist_items_id_seq'::regclass);


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
\.


--
-- Data for Name: accounts; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.accounts (account_id, email, username, password_hash, phone_number, account_status, account_flag_count, auth_provider, avatar_url, refresh_token, refresh_token_expiry_time, is_verified, account_created_at, account_updated_at, account_last_login_at) FROM stdin;
5	anhkct123@gmail.com	anhkct123	\N	0123456789	Banned	2	google	https://res.cloudinary.com/df8i0azc5/image/upload/v1781182423/b4qgmceertszdo80o4uy.png	qs3EE+sK500w4ADQyt3z8/bNxfg6s0TWvqlZx1/jlEJqoWBMch+I5jzcbme+8A10Whm8l8eXABRYa43MOaGfMQ==	2026-06-18 15:53:40.618369	t	2026-06-06 01:17:21.142831	2026-06-11 12:53:27.463334	2026-06-11 15:53:40.612504
7	anhk.ce191266@gmail.com	anhk.ce191266	\N	\N	Active	0	google	https://lh3.googleusercontent.com/a/ACg8ocIR4m1-I_NTSqX5MO_Wr2LQaNfocdn9w1FtTeR1a9jcLnqNfz0=s96-c	\N	\N	t	2026-06-06 02:37:15.181504	2026-06-06 02:37:15.18159	2026-06-06 02:37:15.305395
1	instructor@gmail.com	\N	$2a$11$1ixHeW5PRD1JgYhdgN414urvRL/r/bZZRF.ZqoMo4uh.8QSU7fTLu	\N	Banned	6	local	\N	\N	\N	t	2026-06-06 00:58:43.449323	2026-06-10 09:41:09.330352	2026-06-10 09:34:47.728315
9	anhkclone@gmail.com	anhkclone	\N	\N	Banned	36	google	https://lh3.googleusercontent.com/a/ACg8ocJitViiQeeknMVuLKBqr1xDm3EwtEUaPED4s3NIyYq2gxckrw=s96-c	tscEeCiqrXPcNa2D0uW6931S+2AT0Bdth3+cfy9z4Ne5n0ieY1wnXqiorm4ehXcrg4xAAS85uQRqyuVLp0uhuA==	2026-06-14 03:22:58.757579	t	2026-06-06 12:52:12.499077	2026-06-06 12:52:12.499113	2026-06-07 03:22:58.753062
3	staff@gmail.com	staff	$2a$11$O7PrVmv/I5yxkexhkdrY2OB2tQf5c6Gy9P8hvqLIAF2NO34wt9C3i	+84987654321	active	0	local	\N	olQQy2xuGpzuEhktjQdKgRkGSRwiLhk27+ZnK3TXs6pYXSqhwPlZdH398TCuutV0pfz243XLMWGAnfOICtPboQ==	2026-06-18 12:44:54.400436	t	2026-06-06 00:58:43.465602	2026-06-06 00:58:43.465602	2026-06-06 23:18:02.101474
8	anhkct132@gmail.com	anhkct132	\N	\N	Banned	0	google	https://lh3.googleusercontent.com/a/ACg8ocLjMCteR8D05Kqv4qo74J5jyZ35bU-iyix_t1EedRw8CW9cf_x4=s96-c	j9Ve3BaztSx1PvSa1EboaiGnQqmMXJfVm1Ln8RyXuP4VkFVfYhiAz5nfoVR6SK1SxXcpA0jGBktIkMG/xP4CcQ==	2026-06-18 12:37:34.460648	t	2026-06-06 02:52:13.512313	2026-06-06 02:52:13.512314	2026-06-11 12:37:34.452732
10	anhkct321@gmail.com	anhkct321	$2a$11$T22SdLf1k51TvV3wjrpXTeP7hRY4TUhJTovbqZIi2EcURIeTBYNZW	\N	Active	0	local	\N	\N	\N	t	2026-06-08 00:10:21.293588	2026-06-08 00:11:33.59892	2026-06-11 12:43:59.899102
2	admin@gmail.com	admin	$2a$11$O7PrVmv/I5yxkexhkdrY2OB2tQf5c6Gy9P8hvqLIAF2NO34wt9C3i	+84123456789	active	0	local	\N	x2hHDB/l6B00bPjnPAynKEvmZfcVovpx5DMnFTCqD3DX01B6EK6CqrghPDZzQ1bfkZ9+5dmZmFYelF971NMdtA==	2026-06-18 15:16:12.75862	t	2026-06-06 00:58:43.465602	2026-06-06 00:58:43.465602	2026-06-11 15:16:12.732444
4	anhkclone123@gmail.com	anhkclone123	$2a$11$rfQnuRKxaqAPlJ/zg2G9BO8vJ0hYswehpQObDB/ziPIChgib7WG2.	\N	Active	0	local	\N	\N	\N	t	2026-06-06 01:07:28.299771	2026-06-06 01:07:28.299834	2026-06-08 11:52:04.27856
14	staff2@gmail.com	\N	$2a$11$tBTEk7Ay2rRbviK3JPLeh.ElHuwOQ/dinWyDVo4HRVpMZwr1mzUhq		Active	0	\N	\N	\N	\N	t	2026-06-11 13:38:00.780401	2026-06-11 13:44:05.818134	2026-06-11 13:43:08.398031
11	anhkct123123@gmail.com	anhkct123123	$2a$11$YGmf.eLMvicW3V6fKCKbyeY5BxemRuYaD3p99I3QXgKecTwW09T4.	\N	Active	0	local	\N	\N	\N	t	2026-06-10 09:29:15.409394	2026-06-10 09:43:32.064443	2026-06-10 10:04:48.275208
\.


--
-- Data for Name: ai_models; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.ai_models (model_id, model_name, model_type, model_provider, model_version, model_status, description, model_created_at, model_updated_at, model_path, process_type) FROM stdin;
3	distilbert	embedding_generator	hugging_face	1	active	a language model that was used to generate embeddings	2026-06-06 00:58:43.468719	2026-06-06 00:58:43.468719	distilbert-base-multilingual-cased	text
1	harmful_text_classifier	classifier	meta	1.0.0	active	an ensemble of spam and toxic text classifier that was fine-tuned from distilbert multilingual cased	2026-06-06 00:58:43.468719	2026-06-10 13:35:06.553502	/app/models/spam_1/,/app/models/toxic_3/	text
5	zzz	classifier	meta	1.3.1	active	Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since 1966, when designers at Letraset and James Mosley, the librarian at St Bride Printing Library, took a 1914 Cicero translation and scrambled it to make dummy text for Letraset's Body Type sheets. It has survived not only many decades, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised thanks to these sheets and more recently with desktop publishing software including versions of Lorem Ipsum.	2026-06-10 13:44:11.555893	2026-06-10 13:44:11.555894	asdasd	text
6	asdasd	classifier	meta	1.1.1	active	Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since 1966, when designers at Letraset and James Mosley, the librarian at St Bride Printing Library, took a 1914 Cicero translation and scrambled it to make dummy text for Letraset's Body Type sheets. It has survived not only many decades, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised thanks to these sheets and more recently with desktop publishing software including versions of Lorem Ipsum.	2026-06-10 13:46:36.928585	2026-06-10 14:50:31.029286	123132	text
2	clip	embedding_generator	openai	1.0.0	active	a multimodal model that was used to generate embeddings	2026-06-06 00:58:43.468719	2026-06-10 14:50:33.899423	openai/clip-vit-base-patch32	media
4	test model	classifier	local	1.0.0	active	Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since 1966, when designers at Letraset and James Mosley, the librarian at St Bride Printing Library, took a 1914 Cicero translation and scrambled it to make dummy text for Letraset's Body Type sheets. It has survived not only many decades, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised thanks to these sheets and more recently with desktop publishing software including versions of Lorem Ipsum.	2026-06-10 13:43:03.334793	2026-06-10 14:50:39.558865	/localhost	text
7	asd	classifier	meta	1.0.0	active	<td colspan="5" class="px-6 py-8 text-center text-slate-500">No AI Models found.</td><td colspan="5" class="px-6 py-8 text-center text-slate-500">No AI Models found.</td>	2026-06-10 15:23:41.020115	2026-06-10 15:23:41.020148	asdasd	text
9	asdasd	classifier	meta	1.0.0	active	asdasdasd<td colspan="5" class="px-6 py-8 text-center text-slate-500">No AI Models found.</td><td colspan="5" class="px-6 py-8 text-center text-slate-500">No AI Models found.</td>	2026-06-10 15:24:16.205127	2026-06-10 15:24:16.205127	asdasdasd	text
10	asdasdsa	classifier	meta	1.0.0	active	asd<td colspan="5" class="px-6 py-8 text-center text-slate-500">No AI Models found.</td><td colspan="5" class="px-6 py-8 text-center text-slate-500">No AI Models found.</td>	2026-06-10 15:24:32.768389	2026-06-10 15:24:32.768389	asdasd	text
11	asdasdsa	classifier	openai	1.0.0	active	<td colspan="5" class="px-6 py-8 text-center text-slate-500">No AI Models found.</td><td colspan="5" class="px-6 py-8 text-center text-slate-500">No AI Models found.</td><td colspan="5" class="px-6 py-8 text-center text-slate-500">No AI Models found.</td>	2026-06-10 15:24:45.368024	2026-06-11 15:17:12.977824	asdasdasd	text
8	asdasd	classifier	openai	1.0.0	active	fdsffsdsdsdsdsdsdsdsdsdsdsdsdsdsdsdsdsdsdsdsdsdsdsd	2026-06-10 15:23:58.099116	2026-06-11 15:18:12.961327	asdasdas	text
12	asdasd	classifier	meta	1.0.0	active	sdaasdsadaasdasdasdadadadadadaddad	2026-06-11 15:19:48.842951	2026-06-11 15:19:52.303366	asdad	text
\.


--
-- Data for Name: audit_logs; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.audit_logs (log_id, actor_id, action_type, target_type, target_id, details, ip_address, created_at) FROM stdin;
\.


--
-- Data for Name: avatar_frames; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.avatar_frames (id, name, image_url, description, requirement_type, requirement_value, is_active, created_at) FROM stdin;
2	Khung Admin Tối Thượng	/img/frames/admin_gold.webp	Dành cho quản trị viên cao cấp	MANUAL_GRANT	0	t	2026-06-06 00:58:43.465602
3	Tân Binh Chăm Chỉ	/img/frames/newbie_teal.webp	Hoàn thành khóa học đầu tiên	FINISH_COURSE	1	t	2026-06-06 00:58:43.465602
\.


--
-- Data for Name: cart_items; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cart_items (id, user_id, course_id, added_date, price) FROM stdin;
1	4	3	2026-06-08 11:52:09.084958	9.99
\.


--
-- Data for Name: categories; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.categories (category_id, categories_name, description, created_at, updated_at, category_status) FROM stdin;
1	Design	Courses related to graphic design, UX/UI, 3D modeling, and creative arts.	2026-06-06 00:58:43.447867	2026-06-06 00:58:43.447867	active
2	Development	Software development, programming languages, web development, and mobile app creation.	2026-06-06 00:58:43.447867	2026-06-06 00:58:43.447867	active
3	Business	Business management, leadership, strategy, finance, and entrepreneurship.	2026-06-06 00:58:43.447867	2026-06-06 00:58:43.447867	active
4	Marketing	Digital marketing, SEO, social media advertising, and content strategy.	2026-06-06 00:58:43.447867	2026-06-06 00:58:43.447867	active
5	Photography & Video	Photography, video editing, cinematography, and digital imaging.	2026-06-06 00:58:43.447867	2026-06-06 00:58:43.447867	active
6	Music	Music theory, instrument playing, audio production, and songwriting.	2026-06-06 00:58:43.447867	2026-06-06 00:58:43.447867	active
7	Languages	Learn English, Japanese, Chinese, Spanish, and other languages.	2026-06-06 00:58:43.447867	2026-06-06 00:58:43.447867	active
8	Health & Fitness	Fitness, nutrition, yoga, meditation, and personal well-being.	2026-06-06 00:58:43.447867	2026-06-06 00:58:43.447867	active
9	Data Science	Data science, machine learning, deep learning, and artificial intelligence.	2026-06-06 00:58:43.447867	2026-06-06 00:58:43.447867	active
10	Personal Development	Public speaking, career development, memory improvement, and productivity.	2026-06-06 00:58:43.447867	2026-06-06 00:58:43.447867	active
11	Finance & Investing	Personal finance, stock market investing, trading, and cryptocurrency.	2026-06-06 00:58:43.447867	2026-06-06 00:58:43.447867	active
12	Office Productivity	Microsoft Excel, PowerPoint, Google Workspace, and office tools.	2026-06-06 00:58:43.447867	2026-06-06 00:58:43.447867	active
13	Lifestyle	Cooking, baking, gaming, home improvement, and creative hobbies.	2026-06-06 00:58:43.447867	2026-06-06 00:58:43.447867	active
\.


--
-- Data for Name: chat_participants; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.chat_participants (chat_id, account_id, role, unread_count, last_read_at, joined_at, cleared_at) FROM stdin;
2	3	member	1	2026-06-06 23:17:20.187204	2026-06-06 23:17:20.069616	\N
3	8	member	0	2026-06-06 23:17:32.885918	2026-06-06 23:17:32.863631	\N
3	3	member	1	2026-06-06 23:17:32.866437	2026-06-06 23:17:32.863632	\N
4	9	member	0	2026-06-06 23:17:37.709009	2026-06-06 23:17:37.689801	\N
4	3	member	1	2026-06-06 23:17:37.691343	2026-06-06 23:17:37.689802	\N
2	5	member	0	2026-06-10 05:54:57.875309	2026-06-06 23:17:20.069573	\N
\.


--
-- Data for Name: chats; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.chats (chat_id, chat_name, chat_type, context_type, context_id, created_at, last_message_at) FROM stdin;
2	\N	private	system	\N	2026-06-06 23:17:20.069429	2026-06-06 23:17:27.330092
3	\N	private	system	\N	2026-06-06 23:17:32.863627	2026-06-06 23:17:35.229551
4	\N	private	system	\N	2026-06-06 23:17:37.689797	2026-06-06 23:17:44.261135
\.


--
-- Data for Name: coupons; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.coupons (coupon_id, manager_id, coupon_code, coupon_type, discount_value, min_order_value, start_date, end_date, usage_limit, used_count, is_active) FROM stdin;
\.


--
-- Data for Name: course_ai_usage_logs; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.course_ai_usage_logs (log_id, integration_id, interaction_type, input_json, output_json, latency_ms, token_usage, error_message, log_created_at) FROM stdin;
1	2	moderation	{"course_id": 1, "material_ids": [1, 2], "spam_score_threshold": 0.7, "toxic_score_threshold": 0.7, "similarity_score_threshold": 0.8}	{"step": 1, "stage": 1, "reason": "No semantic duplicates found (threshold: 0.8)", "result": "NO_MATCH", "details": {"material_1": [{"model_id": 3, "similarity_score": 0.0}], "material_2": [{"model_id": 3, "similarity_score": 0.0}]}, "timestamp": "2026-06-06T16:23:53.04303", "flagged_fields": [], "confidence_score": 0}	0.047922134	0	\N	2026-06-06 16:24:03.789614
2	2	moderation	{"course_id": 1, "material_ids": [1, 2], "spam_score_threshold": 0.7, "toxic_score_threshold": 0.7, "similarity_score_threshold": 0.8}	{"step": 1, "stage": 1, "reason": "No semantic duplicates found (threshold: 0.8)", "result": "NO_MATCH", "details": {"material_1": [{"model_id": 3, "similarity_score": 0.0}], "material_2": [{"model_id": 3, "similarity_score": 0.0}]}, "timestamp": "2026-06-06T16:23:53.043225", "flagged_fields": [], "confidence_score": 0}	0.009536743	0	\N	2026-06-06 16:24:03.843622
3	1	moderation	{"course_id": 1, "material_ids": [1, 2], "spam_score_threshold": 0.7, "toxic_score_threshold": 0.7, "similarity_score_threshold": 0.8}	{"step": 1, "stage": 2, "reason": "Harmful content detected in fields: course.what_you_will_learn, course.requirements", "result": "FLAGGED", "details": {"course.title": {"text": "khoa hoc 1", "score": 0.6939351558685303, "reason": "Probable Threat (Low Confidence)", "raw_label": "TOXIC", "latency_ms": 8637.290239334106}, "flagged_count": 2, "lesson_1.title": {"text": "lesson 1...", "score": 0.9848634004592896, "reason": "Inference complete", "raw_label": "SAFE", "latency_ms": 205.7185173034668}, "material_1.title": {"text": "video lessson 1...", "score": 0.9775445759296417, "reason": "Inference complete", "raw_label": "SAFE", "latency_ms": 199.42688941955566}, "material_2.title": {"text": "hoc lieu 1...", "score": 0.9633577466011047, "reason": "Inference complete", "raw_label": "SAFE", "latency_ms": 151.75461769104004}, "course.description": {"text": "<p>mota1</p>", "score": 0.5699307918548584, "reason": "Probable Threat (Low Confidence)", "raw_label": "SPAM", "latency_ms": 296.66733741760254}, "course.requirements": {"text": "<p>9876543210</p><p>1234567890</p>", "score": 0.9982327222824097, "reason": "Inference complete", "raw_label": "SPAM", "latency_ms": 313.3971691131592}, "material_1.description": {"text": "<p>mo ta video lesson 1</p>...", "score": 0.9907041192054749, "reason": "Inference complete", "raw_label": "SAFE", "latency_ms": 203.1991481781006}, "material_2.description": {"text": "<p>mo ta hoc lieu 1</p>...", "score": 0.990565150976181, "reason": "Inference complete", "raw_label": "SAFE", "latency_ms": 125.68330764770508}, "course.what_you_will_learn": {"text": "<p>1324567890</p><p>1234567890</p>", "score": 0.9974059462547302, "reason": "Inference complete", "raw_label": "SPAM", "latency_ms": 384.4952583312988}}, "timestamp": "2026-06-06T16:24:03.575234", "flagged_fields": ["course.what_you_will_learn", "course.requirements"], "confidence_score": 0.99781936}	10517.87	0	\N	2026-06-06 16:24:03.848363
4	5	moderation	{"course_id": 2, "material_ids": [3], "spam_score_threshold": 0.7, "toxic_score_threshold": 0.7, "similarity_score_threshold": 0.8}	{"step": 1, "stage": 1, "reason": "Found a semantic duplication for material_3 on material_1 (cosine similarity: 1.0 > 0.8)", "result": "MATCH_FOUND", "details": {"material_3": [{"model_id": 3, "similarity_score": 1.0}]}, "timestamp": "2026-06-06T16:27:29.369755", "flagged_fields": ["material_3"], "confidence_score": 1}	4.172802	0	\N	2026-06-06 16:27:29.388781
5	5	moderation	{"course_id": 2, "material_ids": [3], "spam_score_threshold": 0.7, "toxic_score_threshold": 0.7, "similarity_score_threshold": 0.8}	{"step": 1, "stage": 1, "reason": "Found a semantic duplication for material_3 on material_1 (cosine similarity: 1.0 > 0.8)", "result": "MATCH_FOUND", "details": {"material_3": [{"model_id": 3, "similarity_score": 1.0}]}, "timestamp": "2026-06-07T00:59:35.338719", "flagged_fields": ["material_3"], "confidence_score": 1}	19.928694	0	\N	2026-06-07 00:59:35.437656
6	8	moderation	{"course_id": 3, "material_ids": [4], "spam_score_threshold": 0.7, "toxic_score_threshold": 0.7, "similarity_score_threshold": 0.8}	{"step": 1, "stage": 1, "reason": "No semantic duplicates found (threshold: 0.8)", "result": "NO_MATCH", "details": {"material_4": [{"model_id": 3, "similarity_score": 0.0}]}, "timestamp": "2026-06-07T03:35:59.036079", "flagged_fields": [], "confidence_score": 0}	16.649723	0	\N	2026-06-07 03:36:17.353699
7	7	moderation	{"course_id": 3, "material_ids": [4], "spam_score_threshold": 0.7, "toxic_score_threshold": 0.7, "similarity_score_threshold": 0.8}	{"step": 1, "stage": 2, "reason": "Harmful content detected in fields: course.title, course.description, course.what_you_will_learn, course.requirements, lesson_3.title, material_4.title", "result": "FLAGGED", "details": {"course.title": {"text": "dasdasd", "score": 0.9948899745941162, "reason": "Inference complete", "raw_label": "TOXIC", "latency_ms": 7750.921249389648}, "flagged_count": 6, "lesson_3.title": {"text": "wt", "score": 0.9999088048934937, "reason": "Inference complete", "raw_label": "TOXIC", "latency_ms": 158.15377235412598}, "material_4.title": {"text": "wt Video", "score": 0.998293936252594, "reason": "Inference complete", "raw_label": "TOXIC", "latency_ms": 176.05996131896973}, "course.description": {"text": "##gb ( 220, 220, 170 ) ; \\" > getElementById < / span > < span style = \\" color : rgb ( 212, 212, 212 ) ; \\" > ( < / span > < span style = \\" color : rgb ( 206, 145, 120 ) ; \\" > ' noti - box ' < / span > < span style = \\" color : rgb ( 212, 212, 212 ) ; \\" > ). < / span > < span style = \\" color : rgb ( 156, 220, 254 ) ; \\" > classList < /", "score": 0.9982631802558899, "reason": "Inference complete", "raw_label": "SPAM", "latency_ms": 4539.417743682861}, "course.requirements": {"text": "##gb ( 220, 220, 170 ) ; \\" > getElementById < / span > < span style = \\" color : rgb ( 212, 212, 212 ) ; \\" > ( < / span > < span style = \\" color : rgb ( 206, 145, 120 ) ; \\" > ' noti - box ' < / span > < span style = \\" color : rgb ( 212, 212, 212 ) ; \\" > ). < / span > < span style = \\" color : rgb ( 156, 220, 254 ) ; \\" > classList < /", "score": 0.9982631802558899, "reason": "Inference complete", "raw_label": "SPAM", "latency_ms": 2760.1447105407715}, "course.what_you_will_learn": {"text": "##gb ( 220, 220, 170 ) ; \\" > getElementById < / span > < span style = \\" color : rgb ( 212, 212, 212 ) ; \\" > ( < / span > < span style = \\" color : rgb ( 206, 145, 120 ) ; \\" > ' noti - box ' < / span > < span style = \\" color : rgb ( 212, 212, 212 ) ; \\" > ). < / span > < span style = \\" color : rgb ( 156, 220, 254 ) ; \\" > classList < /", "score": 0.9982631802558899, "reason": "Inference complete", "raw_label": "SPAM", "latency_ms": 2733.4229946136475}}, "timestamp": "2026-06-07T03:36:17.193408", "flagged_fields": ["course.title", "course.description", "course.what_you_will_learn", "course.requirements", "lesson_3.title", "material_4.title"], "confidence_score": 0.99798036}	18120.99	0	\N	2026-06-07 03:36:17.393238
8	8	moderation	{"course_id": 3, "material_ids": [7, 8], "spam_score_threshold": 0.7, "toxic_score_threshold": 0.7, "similarity_score_threshold": 0.8}	{"step": 1, "stage": 1, "reason": "No semantic duplicates found (threshold: 0.8)", "result": "NO_MATCH", "details": {"material_7": [{"model_id": 3, "similarity_score": 0.0}], "material_8": [{"model_id": 3, "similarity_score": 0.0}]}, "timestamp": "2026-06-08T11:51:23.349978", "flagged_fields": [], "confidence_score": 0}	2.1781921	0	\N	2026-06-08 11:51:40.986981
9	8	moderation	{"course_id": 3, "material_ids": [7, 8], "spam_score_threshold": 0.7, "toxic_score_threshold": 0.7, "similarity_score_threshold": 0.8}	{"step": 1, "stage": 1, "reason": "No semantic duplicates found (threshold: 0.8)", "result": "NO_MATCH", "details": {"material_7": [{"model_id": 3, "similarity_score": 0.0}], "material_8": [{"model_id": 3, "similarity_score": 0.0}]}, "timestamp": "2026-06-08T11:51:23.350284", "flagged_fields": [], "confidence_score": 0}	0.1077652	0	\N	2026-06-08 11:51:41.034797
10	7	moderation	{"course_id": 3, "material_ids": [7, 8], "spam_score_threshold": 0.7, "toxic_score_threshold": 0.7, "similarity_score_threshold": 0.8}	{"step": 1, "stage": 2, "reason": "Harmful content detected in fields: course.title, course.description, course.what_you_will_learn, course.requirements, lesson_5.title", "result": "FLAGGED", "details": {"course.title": {"text": "dasdasd", "score": 0.9948899745941162, "reason": "Inference complete", "raw_label": "TOXIC", "latency_ms": 7301.463842391968}, "flagged_count": 5, "lesson_5.title": {"text": "d", "score": 0.9948511719703674, "reason": "Inference complete", "raw_label": "TOXIC", "latency_ms": 116.63961410522461}, "material_7.title": {"text": "d Video...", "score": 0.9811193645000458, "reason": "Inference complete", "raw_label": "SAFE", "latency_ms": 171.68426513671875}, "material_8.title": {"text": "probability_extraction.png...", "score": 0.957133024930954, "reason": "Inference complete", "raw_label": "SAFE", "latency_ms": 216.82238578796387}, "course.description": {"text": "##b ( 220, 220, 170 ) ; \\" > getElementById < / span > < span style = \\" color : rgb ( 212, 212, 212 ) ; \\" > ( < / span > < span style = \\" color : rgb ( 206, 145, 120 ) ; \\" > ' noti - box ' < / span > < span style = \\" color : rgb ( 212, 212, 212 ) ; \\" > ). < / span > < span style = \\" color : rgb ( 156, 220, 254 ) ; \\" > classList < / span", "score": 0.9984220266342163, "reason": "Inference complete", "raw_label": "SPAM", "latency_ms": 4033.114194869995}, "course.requirements": {"text": "##gb ( 220, 220, 170 ) ; \\" > getElementById < / span > < span style = \\" color : rgb ( 212, 212, 212 ) ; \\" > ( < / span > < span style = \\" color : rgb ( 206, 145, 120 ) ; \\" > ' noti - box ' < / span > < span style = \\" color : rgb ( 212, 212, 212 ) ; \\" > ). < / span > < span style = \\" color : rgb ( 156, 220, 254 ) ; \\" > classList < /", "score": 0.9982631802558899, "reason": "Inference complete", "raw_label": "SPAM", "latency_ms": 2773.9226818084717}, "course.what_you_will_learn": {"text": "##gb ( 220, 220, 170 ) ; \\" > getElementById < / span > < span style = \\" color : rgb ( 212, 212, 212 ) ; \\" > ( < / span > < span style = \\" color : rgb ( 206, 145, 120 ) ; \\" > ' noti - box ' < / span > < span style = \\" color : rgb ( 212, 212, 212 ) ; \\" > ). < / span > < span style = \\" color : rgb ( 156, 220, 254 ) ; \\" > classList < /", "score": 0.9982631802558899, "reason": "Inference complete", "raw_label": "SPAM", "latency_ms": 2810.770273208618}}, "timestamp": "2026-06-08T11:51:40.784683", "flagged_fields": ["course.title", "course.description", "course.what_you_will_learn", "course.requirements", "lesson_5.title"], "confidence_score": 0.99693793}	17424.723	0	\N	2026-06-08 11:51:41.040154
21	1	CourseModeration	{"text": "Learn basic programming from scratch."}	{"result": "pass", "confidence": 0.95}	120.5	45	\N	2026-06-10 15:31:12.042499
22	1	CourseModeration	{"text": "Hack secure systems and steal data."}	{"result": "flagged", "confidence": 0.88}	210	60	\N	2026-06-10 15:31:12.042499
23	1	CourseModeration	{"text": "Advanced Mathematics 101."}	{"result": "pass", "confidence": 0.99}	95.2	20	\N	2026-06-10 15:31:12.042499
24	1	CourseModeration	{"text": "Buy crypto now! Limited time offer!"}	{"result": "manual_audit", "confidence": 0.65}	300.1	150	\N	2026-06-10 15:31:12.042499
25	1	CourseModeration	{"text": "Extremely violent and graphic content."}	{"result": "flagged", "confidence": 0.91}	250	80	\N	2026-06-10 15:31:12.042499
26	1	CourseModeration	{"text": "Comprehensive guide to web design."}	{"result": "pass", "confidence": 0.98}	88.4	15	\N	2026-06-10 15:31:12.042499
27	1	CourseModeration	{"text": "Large payload..."}	\N	5000	0	Connection timeout while reaching AI provider	2026-06-10 15:31:12.042499
28	1	CourseModeration	{"text": "Copied content from competitor."}	{"result": "match_found", "confidence": 0.77}	180	50	\N	2026-06-10 15:31:12.042499
29	1	CourseModeration	{"text": "Original research paper."}	{"result": "no_match", "confidence": 0.82}	110	30	\N	2026-06-10 15:31:12.042499
30	1	CourseModeration	{"text": "Pre-approved curriculum."}	{"result": "approved", "confidence": 0.92}	130.5	40	\N	2026-06-10 15:31:12.042499
\.


--
-- Data for Name: course_exts; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.course_exts (course_id, title_hash, description_hash, what_you_will_learn_hash, requirements_hash, thumbnail_hash) FROM stdin;
1	a344e058af83682637a8100e5627288a	7144eafbd511293412c2ed6df1619955	78c26109e2f10be12276972615ea4ac7	36df66da47101fd91b68764434f079cc	\N
2	5e5b187bbb6b0344ab7d88937786fcbd	d5645766f6a8dfdb084ec725578df50e	f23199a4bc3f9dfa2eccba572f5d4311	838e37ddcf0e31beda55106a9ef294cc	e26f21bb42a8278e6b2890d43364591c
3	0df01ae7dd51cec48fed56952f40842b	5aacbb2090779e282ea3a03181d9939e	37ec035bbf203b6a4ecccd8996d59eb7	37ec035bbf203b6a4ecccd8996d59eb7	e8a2e458058220d6fc91536e3d168331
\.


--
-- Data for Name: course_reports; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.course_reports (course_report_id, reporter_id, course_id, resolver_id, reason, description, course_reports_status, resolution_note, resolved_at, access_granted_until, created_at) FROM stdin;
1	7	1	2	Copyright violation	Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since 1966, when designers at Letraset and James Mosley, the librarian at St Bride Printing Library, took a 1914 Cicero translation and scrambled it to make dummy text for Letraset's Body Type sheets. It has survived not only many decades, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised thanks to these sheets and more recently with desktop publishing software including versions of Lorem Ipsum.	resolved		2026-06-06 02:38:29.329599	\N	2026-06-06 02:37:56.870535
5	5	1	2	Copyright violation	Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since 1966, when designers at Letraset and James Mosley, the librarian at St Bride Printing Library, took a 1914 Cicero translation and scrambled it to make dummy text for Letraset's Body Type sheets. It has survived not only many decades, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised thanks to these sheets and more recently with desktop publishing software including versions of Lorem Ipsum.	resolved		2026-06-06 12:06:02.602537	\N	2026-06-06 12:05:26.812116
3	7	1	2	Copyright violation	Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since 1966, when designers at Letraset and James Mosley, the librarian at St Bride Printing Library, took a 1914 Cicero translation and scrambled it to make dummy text for Letraset's Body Type sheets. It has survived not only many decades, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised thanks to these sheets and more recently with desktop publishing software including versions of Lorem Ipsum.	resolved		2026-06-06 02:50:20.034461	\N	2026-06-06 02:41:48.559194
2	7	1	2	Misleading content	Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since 1966, when designers at Letraset and James Mosley, the librarian at St Bride Printing Library, took a 1914 Cicero translation and scrambled it to make dummy text for Letraset's Body Type sheets. It has survived not only many decades, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised thanks to these sheets and more recently with desktop publishing software including versions of Lorem Ipsum.	resolved		2026-06-06 02:50:29.985577	\N	2026-06-06 02:41:48.493889
4	5	1	2	Misleading content	Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since 1966, when designers at Letraset and James Mosley, the librarian at St Bride Printing Library, took a 1914 Cicero translation and scrambled it to make dummy text for Letraset's Body Type sheets. It has survived not only many decades, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised thanks to these sheets and more recently with desktop publishing software including versions of Lorem Ipsum.	resolved		2026-06-06 12:06:14.084017	\N	2026-06-06 12:05:26.766079
6	5	1	2	Misleading content	Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since 1966, when designers at Letraset and James Mosley, the librarian at St Bride Printing Library, took a 1914 Cicero translation and scrambled it to make dummy text for Letraset's Body Type sheets. It has survived not only many decades, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised thanks to these sheets and more recently with desktop publishing software including versions of Lorem Ipsum.	under_review		2026-06-06 12:25:34.002049	\N	2026-06-06 12:24:19.895323
7	5	1	2	Misleading content	Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since 1966, when designers at Letraset and James Mosley, the librarian at St Bride Printing Library, took a 1914 Cicero translation and scrambled it to make dummy text for Letraset's Body Type sheets. It has survived not only many decades, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised thanks to these sheets and more recently with desktop publishing software including versions of Lorem Ipsum.	resolved		2026-06-06 12:39:06.395219	\N	2026-06-06 12:38:54.120383
10	5	1	2	Spam	Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since 1966, when designers at Letraset and James Mosley, the librarian at St Bride Printing Library, took a 1914 Cicero translation and scrambled it to make dummy text for Letraset's Body Type sheets. It has survived not only many decades, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised thanks to these sheets and more recently with desktop publishing software including versions of Lorem Ipsum.	resolved		2026-06-06 12:48:10.29111	\N	2026-06-06 12:48:00.763474
9	5	1	2	Copyright violation	Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since 1966, when designers at Letraset and James Mosley, the librarian at St Bride Printing Library, took a 1914 Cicero translation and scrambled it to make dummy text for Letraset's Body Type sheets. It has survived not only many decades, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised thanks to these sheets and more recently with desktop publishing software including versions of Lorem Ipsum.	resolved		2026-06-06 12:48:18.01117	\N	2026-06-06 12:48:00.704125
8	5	1	2	Misleading content	Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since 1966, when designers at Letraset and James Mosley, the librarian at St Bride Printing Library, took a 1914 Cicero translation and scrambled it to make dummy text for Letraset's Body Type sheets. It has survived not only many decades, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised thanks to these sheets and more recently with desktop publishing software including versions of Lorem Ipsum.	resolved		2026-06-06 12:48:26.993153	\N	2026-06-06 12:48:00.64522
12	5	1	2	Copyright violation	Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since 1966, when designers at Letraset and James Mosley, the librarian at St Bride Printing Library, took a 1914 Cicero translation and scrambled it to make dummy text for Letraset's Body Type sheets. It has survived not only many decades, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised thanks to these sheets and more recently with desktop publishing software including versions of Lorem Ipsum.	resolved		2026-06-06 12:49:55.708415	\N	2026-06-06 12:49:49.237726
11	5	1	2	Misleading content	Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since 1966, when designers at Letraset and James Mosley, the librarian at St Bride Printing Library, took a 1914 Cicero translation and scrambled it to make dummy text for Letraset's Body Type sheets. It has survived not only many decades, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised thanks to these sheets and more recently with desktop publishing software including versions of Lorem Ipsum.	resolved		2026-06-06 12:49:59.176837	\N	2026-06-06 12:49:49.169688
13	5	1	2	Misleading content	Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since 1966, when designers at Letraset and James Mosley, the librarian at St Bride Printing Library, took a 1914 Cicero translation and scrambled it to make dummy text for Letraset's Body Type sheets. It has survived not only many decades, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised thanks to these sheets and more recently with desktop publishing software including versions of Lorem Ipsum.	resolved		2026-06-06 13:35:43.650724	\N	2026-06-06 13:35:30.648128
14	5	2	3	Misleading content	Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since 1966, when designers at Letraset and James Mosley, the librarian at St Bride Printing Library, took a 1914 Cicero translation and scrambled it to make dummy text for Letraset's Body Type sheets. It has survived not only many decades, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised thanks to these sheets and more recently with desktop publishing software including versions of Lorem Ipsum.	escalated		2026-06-07 00:30:24.682106	\N	2026-06-07 00:30:17.827533
19	5	2	3	Misleading content	document.addEventListener('click', function(e) {\n            const dropdown = document.getElementById('noti-dropdown');\n            if (dropdown && !dropdown.contains(e.target)) {\n                document.getElementById('noti-box').classList.add('hidden');\n            }\n        });	resolved		2026-06-07 03:32:58.829861	\N	2026-06-07 03:32:44.502897
17	5	2	3	Spam	Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since 1966, when designers at Letraset and James Mosley, the librarian at St Bride Printing Library, took a 1914 Cicero translation and scrambled it to make dummy text for Letraset's Body Type sheets. It has survived not only many decades, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised thanks to these sheets and more recently with desktop publishing software including versions of Lorem Ipsum.	resolved		2026-06-07 00:43:44.348199	\N	2026-06-07 00:43:33.972009
15	5	2	3	Misleading content	Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since 1966, when designers at Letraset and James Mosley, the librarian at St Bride Printing Library, took a 1914 Cicero translation and scrambled it to make dummy text for Letraset's Body Type sheets. It has survived not only many decades, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised thanks to these sheets and more recently with desktop publishing software including versions of Lorem Ipsum.	resolved		2026-06-07 00:43:52.812553	\N	2026-06-07 00:43:33.608125
16	5	2	3	Copyright violation	Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since 1966, when designers at Letraset and James Mosley, the librarian at St Bride Printing Library, took a 1914 Cicero translation and scrambled it to make dummy text for Letraset's Body Type sheets. It has survived not only many decades, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised thanks to these sheets and more recently with desktop publishing software including versions of Lorem Ipsum.	resolved		2026-06-07 00:43:49.554944	\N	2026-06-07 00:43:33.933644
18	9	2	3	Misleading content	lorem ipsumlorem ipsumlorem ipsumlorem ipsumlorem ipsumlorem ipsumlorem ipsumlorem ipsum	resolved		2026-06-07 01:30:17.795485	\N	2026-06-07 01:30:08.349625
25	8	3	2	Adult content	dsadas9+89as8d9as8+dasdasd+asd+á8d	rejected		2026-06-11 16:04:50.72858	\N	2026-06-11 16:04:11.162177
20	5	1	2	Misleading content	Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since 1966, when designers at Letraset and James Mosley, the librarian at St Bride Printing Library, took a 1914 Cicero translation and scrambled it to make dummy text for Letraset's Body Type sheets. It has survived not only many decades, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised thanks to these sheets and more recently with desktop publishing software including versions of Lorem Ipsum.	resolved		2026-06-09 04:28:29.603663	\N	2026-06-09 04:27:22.856862
21	8	3	2	Misleading content	sdddddddddddddddddddddddđ8787878d78sa9d798ád798ád798á7d98á7d9s7d9	resolved		2026-06-11 16:02:52.571291	\N	2026-06-11 16:02:29.130345
22	8	3	\N	Misleading content	ád54a65d4a5s64d65as4d65as4d89asd798as7d98as7d98as7d98as7da9s8d	pending	\N	\N	\N	2026-06-11 16:03:26.307641
23	8	3	2	Copyright violation	sadasd7as98d7sa98d798as7d98sa7d98as798dasd7s98a7d8sa9d9	resolved		2026-06-11 16:05:23.366771	\N	2026-06-11 16:04:11.042129
24	8	3	2	Spam	sad7as98d7as98d7as98d798as7d98a7s98d7as98d7s98a7d98as	resolved		2026-06-11 16:05:16.628872	\N	2026-06-11 16:04:11.101653
\.


--
-- Data for Name: course_review_moderation_logs; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.course_review_moderation_logs (log_id, model_id, course_review_id, input_json, output_json, latency_ms, error_message, log_created_at) FROM stdin;
1	1	1	{"comment": "This is a great course, highly recommended!"}	{"result": "pass", "toxicity": 0.01}	85	\N	2026-06-10 15:31:12.042499
2	1	2	{"comment": "This is terrible and you are stupid."}	{"result": "flagged", "toxicity": 0.95}	150.2	\N	2026-06-10 15:31:12.042499
3	1	3	{"comment": "I learned a lot, thanks."}	{"result": "pass", "toxicity": 0.05}	90.1	\N	2026-06-10 15:31:12.042499
4	2	4	{"comment": "Buy my crypto at this link!"}	{"result": "manual_audit", "toxicity": 0.60}	200	\N	2026-06-10 15:31:12.042499
5	3	5	{"comment": "Not bad for the price."}	{"result": "pass", "toxicity": 0.10}	75.5	\N	2026-06-10 15:31:12.042499
6	1	6	{"comment": "I hate everything about this."}	{"result": "manual_audit", "toxicity": 0.70}	120.3	\N	2026-06-10 15:31:12.042499
7	3	7	{"comment": "Long review text..."}	\N	3000	API Error: Model server temporarily unavailable	2026-06-10 15:31:12.042499
8	2	8	{"comment": "Very informative and well structured."}	{"result": "pass", "toxicity": 0.02}	88	\N	2026-06-10 15:31:12.042499
9	1	9	{"comment": "Could be better, a bit dry."}	{"result": "pass", "toxicity": 0.15}	92.5	\N	2026-06-10 15:31:12.042499
10	3	10	{"comment": "Spam link http://malicious.com"}	{"result": "flagged", "toxicity": 0.85}	140	\N	2026-06-10 15:31:12.042499
11	10	1	{"comment": "This is a great course, highly recommended!"}	{"result": "pass", "toxicity": 0.01}	85	\N	2026-06-10 15:32:44.102717
12	9	2	{"comment": "This is terrible and you are stupid."}	{"result": "flagged", "toxicity": 0.95}	150.2	\N	2026-06-10 15:32:44.102717
\.


--
-- Data for Name: course_review_reports; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.course_review_reports (course_review_report_id, reporter_id, course_review_id, resolver_id, reason, description, user_reports_status, resolution_note, resolved_at, access_granted_until, created_at) FROM stdin;
2	1	1	2	offensive		resolved		2026-06-06 01:36:16.773901	\N	2026-06-06 01:35:36.537625
1	5	1	2	spam		resolved		2026-06-06 01:38:02.115149	\N	2026-06-06 01:35:17.395408
4	1	1	2	fake		resolved		2026-06-06 01:40:09.511218	\N	2026-06-06 01:39:53.071299
3	1	1	2	spam		resolved		2026-06-06 01:40:11.87068	\N	2026-06-06 01:39:49.159297
5	8	3	3	spam		escalated	anh ơi giải quyết giùm em th này\n	2026-06-06 04:04:44.96133	\N	2026-06-06 04:03:55.397895
8	8	4	2	other		resolved		2026-06-06 07:25:58.599975	\N	2026-06-06 07:24:57.926401
29	5	12	2	offensive		resolved		2026-06-11 15:22:38.820914	\N	2026-06-09 04:30:30.081652
7	8	4	2	fake		resolved		2026-06-06 07:40:14.931869	\N	2026-06-06 07:24:51.484343
10	8	4	2	fake		resolved		2026-06-06 07:41:01.152151	\N	2026-06-06 07:40:50.913493
9	8	4	2	offensive		resolved		2026-06-06 07:42:04.739863	\N	2026-06-06 07:40:46.81433
34	8	13	2	spam		resolved		2026-06-11 15:23:22.749755	\N	2026-06-11 15:23:07.14203
11	5	5	2	spam		resolved		2026-06-06 12:27:48.061873	\N	2026-06-06 12:26:57.514265
6	8	4	2	spam		rejected		2026-06-06 12:28:13.551372	\N	2026-06-06 07:24:47.526167
12	5	5	3	spam		resolved		2026-06-06 23:32:15.496544	\N	2026-06-06 23:31:25.15042
13	5	5	3	spam		resolved		2026-06-07 00:01:52.779184	\N	2026-06-06 23:58:54.827119
35	8	13	2	spam		resolved		2026-06-11 15:25:10.023745	\N	2026-06-11 15:24:55.983724
16	5	5	3	spam		resolved		2026-06-07 00:21:24.57737	\N	2026-06-07 00:21:18.805498
15	5	6	3	spam		resolved		2026-06-07 00:21:27.804361	\N	2026-06-07 00:21:16.552767
14	5	7	3	spam		resolved		2026-06-07 00:21:32.234397	\N	2026-06-07 00:21:13.984138
17	5	8	3	spam		rejected		2026-06-07 00:29:25.596441	\N	2026-06-07 00:28:47.232191
20	5	8	3	fake		resolved		2026-06-07 01:45:57.339061	\N	2026-06-07 01:45:49.102108
19	5	8	3	offensive		resolved		2026-06-07 01:46:05.43733	\N	2026-06-07 01:45:45.124975
38	8	14	2	fake		resolved		2026-06-11 15:52:41.95803	\N	2026-06-11 15:52:03.926465
18	5	8	3	spam		resolved		2026-06-07 01:46:18.795259	\N	2026-06-07 01:45:40.695902
21	5	8	3	fake		resolved		2026-06-07 01:46:59.513117	\N	2026-06-07 01:46:47.324629
37	8	14	2	offensive		resolved		2026-06-11 15:53:18.702682	\N	2026-06-11 15:51:59.03879
22	5	8	3	spam		resolved		2026-06-07 01:48:03.61895	\N	2026-06-07 01:47:57.490601
25	5	8	3	fake		resolved		2026-06-07 01:57:58.368027	\N	2026-06-07 01:57:39.681246
24	5	8	3	offensive		resolved		2026-06-07 01:58:23.597866	\N	2026-06-07 01:57:36.01879
23	5	8	3	spam		resolved		2026-06-07 02:00:32.84991	\N	2026-06-07 01:57:31.817571
40	8	14	2	offensive		resolved		2026-06-11 15:55:34.785774	\N	2026-06-11 15:55:19.32538
26	5	9	3	spam		resolved		2026-06-07 02:01:34.049616	\N	2026-06-07 02:00:20.711521
28	5	10	3	offensive		resolved		2026-06-07 03:22:22.809436	\N	2026-06-07 03:21:41.95054
39	8	14	2	fake		resolved		2026-06-11 15:55:44.111555	\N	2026-06-11 15:55:15.810983
27	5	10	3	spam		resolved		2026-06-07 03:22:43.186426	\N	2026-06-07 03:21:37.218541
36	8	14	2	spam		rejected		2026-06-11 15:56:17.140665	\N	2026-06-11 15:51:55.053647
31	5	12	2	fake		resolved		2026-06-09 04:31:07.636681	\N	2026-06-09 04:30:38.716796
30	5	12	2	spam		rejected		2026-06-09 09:48:07.285946	\N	2026-06-09 04:30:34.763497
33	8	13	2	offensive		resolved		2026-06-11 15:22:05.767623	\N	2026-06-11 15:21:43.396682
32	8	13	2	spam		resolved		2026-06-11 15:22:21.426249	\N	2026-06-11 15:21:34.629028
\.


--
-- Data for Name: course_reviews; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.course_reviews (course_review_id, enrollment_id, rating, comment, course_review_status, created_at, updated_at, is_removed) FROM stdin;
1	2	4.00	ádasdasd	violating	2026-06-06 01:35:05.327401	2026-06-06 01:40:09.514221	t
2	2	4.00	ádasdzzzzweqweqwe	removed	2026-06-06 01:39:41.431491	2026-06-06 02:27:22.378786	t
3	2	4.00	asdasd	removed	2026-06-06 02:27:34.875765	2026-06-06 07:24:19.40324	t
4	2	3.00	asdasdasd	violating	2026-06-06 07:24:32.136768	2026-06-06 07:42:04.741181	t
5	4	5.00	ádzzz	violating	2026-06-06 12:26:48.105177	2026-06-07 00:21:24.57959	t
6	4	4.00	asd	violating	2026-06-07 00:19:50.252447	2026-06-07 00:21:27.805742	t
7	4	3.00	zxczxc	violating	2026-06-07 00:19:53.006136	2026-06-07 00:21:32.235774	t
8	5	5.00	aaa	violating	2026-06-07 00:28:33.746006	2026-06-07 02:00:32.851643	t
9	5	4.00	dsdd	violating	2026-06-07 02:00:00.875246	2026-06-07 02:01:34.051112	t
10	5	5.00	ádasdasd	violating	2026-06-07 03:21:04.396418	2026-06-07 03:22:43.188221	t
11	5	5.00	asdasd	ok	2026-06-07 03:24:42.486705	2026-06-07 03:24:42.486706	f
12	7	5.00	ádadasdádasd	violating	2026-06-09 04:30:12.28746	2026-06-11 15:22:38.824089	t
13	13	5.00	asdasdasd	violating	2026-06-11 15:21:22.799474	2026-06-11 15:25:10.025565	t
14	13	5.00	sad	violating	2026-06-11 15:51:44.921339	2026-06-11 15:55:44.114277	t
\.


--
-- Data for Name: courses; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.courses (course_id, instructor_id, category_id, coupon_id, title, description, price, course_thumbnail_url, created_at, updated_at, course_status, course_flag_count, what_you_will_learn, requirements, moderation_feedback, last_approved_at, is_removed, threat_level) FROM stdin;
1	1	3	\N	khoa hoc 1	<p>mota1</p>	0.00	data:image/png;base64,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	2026-06-06 01:01:41.225128	2026-06-07 03:32:01.545839	archived	3	<p>1324567890</p><p>1234567890</p>	<p>9876543210</p><p>1234567890</p>	[VIOLATION FLAG #3] d	2026-06-06 16:28:20.866497	f	4
3	5	2	\N	dasdasd	<p><span style="color: rgb(156, 220, 254);">document</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">addEventListener</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(206, 145, 120);">'click'</span><span style="color: rgb(212, 212, 212);">, </span><span style="color: rgb(86, 156, 214);">function</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(156, 220, 254);">e</span><span style="color: rgb(212, 212, 212);">) {</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</span><span style="color: rgb(86, 156, 214);">const</span><span style="color: rgb(212, 212, 212);"> </span><span style="color: rgb(79, 193, 255);">dropdown</span><span style="color: rgb(212, 212, 212);"> = </span><span style="color: rgb(156, 220, 254);">document</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">getElementById</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(206, 145, 120);">'noti-dropdown'</span><span style="color: rgb(212, 212, 212);">);</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</span><span style="color: rgb(197, 134, 192);">if</span><span style="color: rgb(212, 212, 212);"> (</span><span style="color: rgb(156, 220, 254);">dropdown</span><span style="color: rgb(212, 212, 212);"> &amp;&amp; !</span><span style="color: rgb(156, 220, 254);">dropdown</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">contains</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(156, 220, 254);">e</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(156, 220, 254);">target</span><span style="color: rgb(212, 212, 212);">){</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</span><span style="color: rgb(156, 220, 254);">document</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">getElementById</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(206, 145, 120);">'noti-box'</span><span style="color: rgb(212, 212, 212);">).</span><span style="color: rgb(156, 220, 254);">classList</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">add</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(206, 145, 120);">'hidden'</span><span style="color: rgb(212, 212, 212);">);</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;});</span></p>	0.00	https://res.cloudinary.com/df8i0azc5/image/upload/v1780803264/hiwedslkr9nwhcmk5kuh.png	2026-06-07 03:34:25.375364	2026-06-11 15:58:24.801902	archived	3	<p><span style="color: rgb(156, 220, 254);">document</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">addEventListener</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(206, 145, 120);">'click'</span><span style="color: rgb(212, 212, 212);">, </span><span style="color: rgb(86, 156, 214);">function</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(156, 220, 254);">e</span><span style="color: rgb(212, 212, 212);">) {</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</span><span style="color: rgb(86, 156, 214);">const</span><span style="color: rgb(212, 212, 212);"> </span><span style="color: rgb(79, 193, 255);">dropdown</span><span style="color: rgb(212, 212, 212);"> = </span><span style="color: rgb(156, 220, 254);">document</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">getElementById</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(206, 145, 120);">'noti-dropdown'</span><span style="color: rgb(212, 212, 212);">);</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</span><span style="color: rgb(197, 134, 192);">if</span><span style="color: rgb(212, 212, 212);"> (</span><span style="color: rgb(156, 220, 254);">dropdown</span><span style="color: rgb(212, 212, 212);"> &amp;&amp; !</span><span style="color: rgb(156, 220, 254);">dropdown</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">contains</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(156, 220, 254);">e</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(156, 220, 254);">target</span><span style="color: rgb(212, 212, 212);">)) {</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</span><span style="color: rgb(156, 220, 254);">document</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">getElementById</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(206, 145, 120);">'noti-box'</span><span style="color: rgb(212, 212, 212);">).</span><span style="color: rgb(156, 220, 254);">classList</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">add</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(206, 145, 120);">'hidden'</span><span style="color: rgb(212, 212, 212);">);</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;});</span></p>	<p><span style="color: rgb(156, 220, 254);">document</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">addEventListener</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(206, 145, 120);">'click'</span><span style="color: rgb(212, 212, 212);">, </span><span style="color: rgb(86, 156, 214);">function</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(156, 220, 254);">e</span><span style="color: rgb(212, 212, 212);">) {</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</span><span style="color: rgb(86, 156, 214);">const</span><span style="color: rgb(212, 212, 212);"> </span><span style="color: rgb(79, 193, 255);">dropdown</span><span style="color: rgb(212, 212, 212);"> = </span><span style="color: rgb(156, 220, 254);">document</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">getElementById</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(206, 145, 120);">'noti-dropdown'</span><span style="color: rgb(212, 212, 212);">);</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</span><span style="color: rgb(197, 134, 192);">if</span><span style="color: rgb(212, 212, 212);"> (</span><span style="color: rgb(156, 220, 254);">dropdown</span><span style="color: rgb(212, 212, 212);"> &amp;&amp; !</span><span style="color: rgb(156, 220, 254);">dropdown</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">contains</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(156, 220, 254);">e</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(156, 220, 254);">target</span><span style="color: rgb(212, 212, 212);">)) {</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</span><span style="color: rgb(156, 220, 254);">document</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">getElementById</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(206, 145, 120);">'noti-box'</span><span style="color: rgb(212, 212, 212);">).</span><span style="color: rgb(156, 220, 254);">classList</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">add</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(206, 145, 120);">'hidden'</span><span style="color: rgb(212, 212, 212);">);</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;});</span></p>	\N	2026-06-11 15:58:24.801904	f	4
2	1	2	\N	khoa hoc 1 ádasdasd	<p>mo ta 1</p>	0.00	https://res.cloudinary.com/df8i0azc5/image/upload/v1780763171/cjblhsioa6uxpme0dbnr.png	2026-06-06 16:26:20.372168	2026-06-09 04:40:07.236328	rejected	2	<p>dau ra 2</p><p><strong style="color: rgb(0, 0, 0); background-color: rgb(255, 255, 255);">Lorem Ipsum</strong><span style="color: rgb(0, 0, 0); background-color: rgb(255, 255, 255);"> is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since 1966, when designers at Letraset and James Mosley, the librarian at St Bride Printing Library, took a 1914 Cicero translation and scrambled it to make dummy text for Letraset's Body Type sheets. It has survived not only many decades, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised thanks to these sheets and more recently with desktop publishing software including versions of Lorem Ipsum.</span></p>	<p>dau vao 1</p><p><strong style="color: rgb(0, 0, 0); background-color: rgb(255, 255, 255);">Lorem Ipsum</strong><span style="color: rgb(0, 0, 0); background-color: rgb(255, 255, 255);"> is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since 1966, when designers at Letraset and James Mosley, the librarian at St Bride Printing Library, took a 1914 Cicero translation and scrambled it to make dummy text for Letraset's Body Type sheets. It has survived not only many decades, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised thanks to these sheets and more recently with desktop publishing software including versions of Lorem Ipsum.</span></p>	[@Description] n	2026-06-07 00:57:37.045255	f	4
\.


--
-- Data for Name: courses_ai_integrations; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.courses_ai_integrations (id, model_id, course_id, role, is_enabled, config_json, assigned_at) FROM stdin;
1	1	1	classifier_text	t	{"spam": 0.7, "toxic": 0.7, "similarity": 0.8}	2026-06-06 16:23:48.087048
2	3	1	embedding_generator_text	t	{"spam": 0.7, "toxic": 0.7, "similarity": 0.8}	2026-06-06 16:23:48.111517
3	2	1	embedding_generator_media	t	{"spam": 0.7, "toxic": 0.7, "similarity": 0.8}	2026-06-06 16:23:48.116003
4	1	2	classifier_text	t	{"spam": 0.7, "toxic": 0.7, "similarity": 0.8}	2026-06-06 16:27:25.993752
5	3	2	embedding_generator_text	t	{"spam": 0.7, "toxic": 0.7, "similarity": 0.8}	2026-06-06 16:27:25.998506
6	2	2	embedding_generator_media	t	{"spam": 0.7, "toxic": 0.7, "similarity": 0.8}	2026-06-06 16:27:26.002056
7	1	3	classifier_text	t	{"spam": 0.7, "toxic": 0.7, "similarity": 0.8}	2026-06-07 03:35:53.631071
8	3	3	embedding_generator_text	t	{"spam": 0.7, "toxic": 0.7, "similarity": 0.8}	2026-06-07 03:35:53.645993
9	2	3	embedding_generator_media	t	{"spam": 0.7, "toxic": 0.7, "similarity": 0.8}	2026-06-07 03:35:53.650288
\.


--
-- Data for Name: enrollments; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.enrollments (enrollment_id, user_id, course_id, title, description, completed_date, is_completed, enroll_date, last_accessed_at, enrollment_status) FROM stdin;
1	4	1	khoa hoc 1	\N	\N	f	2026-06-06	2026-06-06 01:08:01.226158	active
2	5	1	khoa hoc 1	\N	2026-06-06	t	2026-06-06	2026-06-06 01:17:25.098695	active
3	7	1	khoa hoc 1	\N	2026-06-06	t	2026-06-06	2026-06-06 02:37:19.14538	active
4	8	1	khoa hoc 1	\N	2026-06-06	t	2026-06-06	2026-06-06 02:52:30.405677	active
5	9	2	khoa hoc 2	\N	2026-06-07	t	2026-06-07	2026-06-07 00:28:16.79905	active
6	5	2	khoa hoc 2	\N	2026-06-07	t	2026-06-07	2026-06-07 00:29:06.264883	active
7	1	3	dasdasd	\N	2026-06-07	t	2026-06-07	2026-06-07 03:36:46.148556	active
8	11	3	dasdasd	\N	\N	f	2026-06-10	2026-06-10 10:05:13.322321	active
9	\N	3	dasdasd	<p><span style="color: rgb(156, 220, 254);">document</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">addEventListener</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(206, 145, 120);">'click'</span><span style="color: rgb(212, 212, 212);">, </span><span style="color: rgb(86, 156, 214);">function</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(156, 220, 254);">e</span><span style="color: rgb(212, 212, 212);">) {</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</span><span style="color: rgb(86, 156, 214);">const</span><span style="color: rgb(212, 212, 212);"> </span><span style="color: rgb(79, 193, 255);">dropdown</span><span style="color: rgb(212, 212, 212);"> = </span><span style="color: rgb(156, 220, 254);">document</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">getElementById</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(206, 145, 120);">'noti-dropdown'</span><span style="color: rgb(212, 212, 212);">);</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</span><span style="color: rgb(197, 134, 192);">if</span><span style="color: rgb(212, 212, 212);"> (</span><span style="color: rgb(156, 220, 254);">dropdown</span><span style="color: rgb(212, 212, 212);"> &amp;&amp; !</span><span style="color: rgb(156, 220, 254);">dropdown</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">contains</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(156, 220, 254);">e</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(156, 220, 254);">target</span><span style="color: rgb(212, 212, 212);">){</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</span><span style="color: rgb(156, 220, 254);">document</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">getElementById</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(206, 145, 120);">'noti-box'</span><span style="color: rgb(212, 212, 212);">).</span><span style="color: rgb(156, 220, 254);">classList</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">add</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(206, 145, 120);">'hidden'</span><span style="color: rgb(212, 212, 212);">);</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;});</span></p>	\N	f	2026-06-11	2026-06-11 12:22:55.4357	active
11	\N	3	dasdasd	<p><span style="color: rgb(156, 220, 254);">document</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">addEventListener</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(206, 145, 120);">'click'</span><span style="color: rgb(212, 212, 212);">, </span><span style="color: rgb(86, 156, 214);">function</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(156, 220, 254);">e</span><span style="color: rgb(212, 212, 212);">) {</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</span><span style="color: rgb(86, 156, 214);">const</span><span style="color: rgb(212, 212, 212);"> </span><span style="color: rgb(79, 193, 255);">dropdown</span><span style="color: rgb(212, 212, 212);"> = </span><span style="color: rgb(156, 220, 254);">document</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">getElementById</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(206, 145, 120);">'noti-dropdown'</span><span style="color: rgb(212, 212, 212);">);</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</span><span style="color: rgb(197, 134, 192);">if</span><span style="color: rgb(212, 212, 212);"> (</span><span style="color: rgb(156, 220, 254);">dropdown</span><span style="color: rgb(212, 212, 212);"> &amp;&amp; !</span><span style="color: rgb(156, 220, 254);">dropdown</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">contains</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(156, 220, 254);">e</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(156, 220, 254);">target</span><span style="color: rgb(212, 212, 212);">){</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</span><span style="color: rgb(156, 220, 254);">document</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">getElementById</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(206, 145, 120);">'noti-box'</span><span style="color: rgb(212, 212, 212);">).</span><span style="color: rgb(156, 220, 254);">classList</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">add</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(206, 145, 120);">'hidden'</span><span style="color: rgb(212, 212, 212);">);</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;});</span></p>	\N	f	2026-06-11	2026-06-11 12:40:52.932279	active
12	10	3	dasdasd	<p><span style="color: rgb(156, 220, 254);">document</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">addEventListener</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(206, 145, 120);">'click'</span><span style="color: rgb(212, 212, 212);">, </span><span style="color: rgb(86, 156, 214);">function</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(156, 220, 254);">e</span><span style="color: rgb(212, 212, 212);">) {</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</span><span style="color: rgb(86, 156, 214);">const</span><span style="color: rgb(212, 212, 212);"> </span><span style="color: rgb(79, 193, 255);">dropdown</span><span style="color: rgb(212, 212, 212);"> = </span><span style="color: rgb(156, 220, 254);">document</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">getElementById</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(206, 145, 120);">'noti-dropdown'</span><span style="color: rgb(212, 212, 212);">);</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</span><span style="color: rgb(197, 134, 192);">if</span><span style="color: rgb(212, 212, 212);"> (</span><span style="color: rgb(156, 220, 254);">dropdown</span><span style="color: rgb(212, 212, 212);"> &amp;&amp; !</span><span style="color: rgb(156, 220, 254);">dropdown</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">contains</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(156, 220, 254);">e</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(156, 220, 254);">target</span><span style="color: rgb(212, 212, 212);">){</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</span><span style="color: rgb(156, 220, 254);">document</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">getElementById</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(206, 145, 120);">'noti-box'</span><span style="color: rgb(212, 212, 212);">).</span><span style="color: rgb(156, 220, 254);">classList</span><span style="color: rgb(212, 212, 212);">.</span><span style="color: rgb(220, 220, 170);">add</span><span style="color: rgb(212, 212, 212);">(</span><span style="color: rgb(206, 145, 120);">'hidden'</span><span style="color: rgb(212, 212, 212);">);</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}</span></p><p><span style="color: rgb(212, 212, 212);">&nbsp;&nbsp;&nbsp;&nbsp;});</span></p>	\N	f	2026-06-11	2026-06-11 12:46:56.211847	active
13	5	3	dasdasd	\N	\N	t	2026-06-11	2026-06-11 15:21:22.72517	active
14	8	3	dasdasd	\N	\N	f	2026-06-11	2026-06-11 16:02:17.541071	active
\.


--
-- Data for Name: gifts; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.gifts (gift_id, order_item_id, sender_id, recipient_email, recipient_name, gift_message, card_theme, redemption_token, is_claimed, claimed_by_user_id, claimed_at, delivery_status, created_at, updated_at) FROM stdin;
1	2	8	bachitarffhuiiy@gmail.com	keein an	asdasdasdasdasdasdasd\r\nncnncncncnnc'\r\nasdiahsdhasid	thanks	9292cb930d9a483396faa769f23af4ec	t	\N	2026-06-11 12:22:55.461367	sent	2026-06-11 12:21:34.032684	2026-06-11 12:22:55.461395
2	3	8	bachitarffhuiiy@gmail.com	keein an	asdasdasd	classic	a684161999824dc0b401f7fb28e1c493	t	\N	2026-06-11 12:40:52.936109	sent	2026-06-11 12:38:02.479225	2026-06-11 12:40:52.93611
3	4	8	bachitarffhuiiy@gmail.com	keein an	asdasdasdasdasd	classic	85238230febb4227979e0151519d3271	t	10	2026-06-11 12:46:56.215059	sent	2026-06-11 12:42:42.997632	2026-06-11 12:46:56.21506
\.


--
-- Data for Name: instructor_payouts; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.instructor_payouts (payout_id, transaction_id, instructor_id, payout_amount, payout_date, is_paid, payout_status, stripe_transfer_id, stripe_payout_id, paid_to_bank_at) FROM stdin;
1	1	5	7.52	2026-06-24 10:05:13.352395	f	pending	\N	\N	\N
2	2	5	7.52	2026-07-15 00:00:00	f	pending	\N	\N	\N
3	3	5	7.52	2026-07-15 00:00:00	f	pending	\N	\N	\N
4	4	5	7.52	2026-07-15 00:00:00	f	pending	\N	\N	\N
\.


--
-- Data for Name: instructors; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.instructors (instructor_id, stripe_account_id, stripe_onboarding_status, payouts_enabled, charges_enabled, professional_title, expertise_categories, linkedin_url, youtube_url, facebook_url, document_url, approval_status, stripe_country, rejection_reason) FROM stdin;
1	\N	\N	f	f	\N	\N	\N	\N	\N	\N	Approved	\N	\N
5	acct_1Tg1fAF2WS38mDKv	Active	t	t	asdsa	dasdasd	\N	\N	\N	https://res.cloudinary.com/df8i0azc5/image/upload/v1780803238/tklkhvjjklaudasrblkd.png	Approved	US	\N
\.


--
-- Data for Name: learning_materials; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.learning_materials (material_id, lesson_id, title, description, created_at, updated_at, learning_status, moderation_feedback, material_url, material_metadata, cloud_public_id) FROM stdin;
1	1	video lessson 1	<p>mo ta video lesson 1</p>	2026-06-06 01:11:43.197817	2026-06-06 12:35:53.486373	active	\N	https://res.cloudinary.com/df8i0azc5/video/upload/v1780708312/linocwtygs9twr6ld0o7.mp4	{"duration": 20, "file_size": 690969, "file_type": "video", "page_count": null, "file_extension": "mp4"}	\N
2	1	hoc lieu 1	<p>mo ta hoc lieu 1</p>	2026-06-06 01:13:11.579671	2026-06-06 01:13:41.681987	active	\N	https://res.cloudinary.com/df8i0azc5/raw/upload/v1780708401/ss8vopgqla6hu8vwejpc.txt	{"duration": null, "file_size": 349, "file_type": "document", "page_count": null, "file_extension": null}	\N
3	2	zzz Video	\N	2026-06-06 16:27:19.054297	2026-06-06 16:27:19.054324	active	\N	https://res.cloudinary.com/df8i0azc5/video/upload/v1780763237/sjuisc422r8vft4n7dkn.mp4	{"duration": 20, "file_size": 690969, "file_type": "video", "page_count": null, "file_extension": "mp4"}	\N
6	5	d Video	\N	2026-06-07 05:26:03.258981	2026-06-07 05:26:17.061377	active	\N	https://res.cloudinary.com/df8i0azc5/video/upload/v1780809964/trash/pzp4mo59bdmsltrgt7pn.mp4	{"duration": 10, "file_size": 2173848, "file_type": "video", "page_count": null, "file_extension": "mp4"}	pzp4mo59bdmsltrgt7pn
7	5	d Video	\N	2026-06-07 05:26:17.061411	2026-06-07 05:26:17.061411	active	\N	https://res.cloudinary.com/df8i0azc5/video/upload/v1780809975/lswzs2xf81ckoyn6rdcv.mp4	{"duration": 15, "file_size": 1313202, "file_type": "video", "page_count": null, "file_extension": "mp4"}	\N
4	3	wt Video	\N	2026-06-07 03:35:49.323391	2026-06-07 05:07:17.236823	removed	\N	https://res.cloudinary.com/df8i0azc5/video/upload/v1780803349/trash/t9u1qwtbzyzskjbnnl7y.mp4	{"duration": 10, "file_size": 2173848, "file_type": "video", "page_count": null, "file_extension": "mp4"}	t9u1qwtbzyzskjbnnl7y
5	3		<p><br></p>	2026-06-07 04:05:44.757144	2026-06-07 05:07:18.338861	removed	\N	https://res.cloudinary.com/df8i0azc5/raw/upload/v1780805146/trash/kjfdq39l3ieygarbaevl.png	{"duration": null, "file_size": 149747, "file_type": "document", "page_count": null, "file_extension": null}	kjfdq39l3ieygarbaevl.png
8	5	probability_extraction.png	\N	2026-06-07 05:26:22.883008	2026-06-07 05:26:22.883008	active	\N	https://res.cloudinary.com/df8i0azc5/raw/upload/v1780809984/uhthdvyp4en74vr5wsb6.png	{"duration": null, "file_size": 154457, "file_type": "document", "page_count": null, "file_extension": null}	\N
\.


--
-- Data for Name: lesson_review_moderation_logs; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.lesson_review_moderation_logs (log_id, model_id, lesson_review_id, input_json, output_json, latency_ms, error_message, log_created_at) FROM stdin;
1	1	1	{"comment": "Clear audio and great visuals."}	{"result": "pass", "toxicity": 0.01}	80	\N	2026-06-10 15:31:12.042499
2	2	2	{"comment": "A bit boring."}	{"result": "pass", "toxicity": 0.20}	90.5	\N	2026-06-10 15:31:12.042499
3	1	3	{"comment": "Terrible explanation, makes no sense."}	{"result": "manual_audit", "toxicity": 0.55}	110	\N	2026-06-10 15:31:12.042499
4	3	4	{"comment": "Perfect pacing."}	{"result": "pass", "toxicity": 0.00}	70.2	\N	2026-06-10 15:31:12.042499
5	2	5	{"comment": "The instructor talks too fast."}	{"result": "pass", "toxicity": 0.10}	85.5	\N	2026-06-10 15:31:12.042499
6	1	6	{"comment": "You suck!"}	{"result": "flagged", "toxicity": 0.90}	130.4	\N	2026-06-10 15:31:12.042499
7	3	7	{"comment": "Good examples provided."}	{"result": "pass", "toxicity": 0.05}	95	\N	2026-06-10 15:31:12.042499
8	2	8	{"comment": "There is an audio sync issue at 2:00."}	{"result": "pass", "toxicity": 0.08}	88.8	\N	2026-06-10 15:31:12.042499
9	1	9	{"comment": "Test comment"}	\N	2500	Timeout error reading from socket	2026-06-10 15:31:12.042499
10	3	10	{"comment": "Excellent lesson, best one so far!"}	{"result": "pass", "toxicity": 0.02}	75	\N	2026-06-10 15:31:12.042499
11	10	1	{"comment": "Clear audio and great visuals."}	{"result": "pass", "toxicity": 0.01}	80	\N	2026-06-10 15:32:42.066214
12	9	2	{"comment": "A bit boring."}	{"result": "pass", "toxicity": 0.20}	90.5	\N	2026-06-10 15:32:42.066214
\.


--
-- Data for Name: lesson_review_reports; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.lesson_review_reports (lesson_review_report_id, reporter_id, lesson_review_id, resolver_id, reason, description, user_reports_status, resolution_note, resolved_at, access_granted_until, created_at) FROM stdin;
2	5	3	2	offensive	\N	resolved		2026-06-06 01:21:18.579302	\N	2026-06-06 01:21:04.123264
1	5	3	2	spam	\N	resolved		2026-06-06 01:21:23.119259	\N	2026-06-06 01:18:35.900594
3	1	2	2	spam	\N	resolved		2026-06-06 01:22:46.614383	\N	2026-06-06 01:22:34.26916
4	1	2	2	spam	\N	under_review		2026-06-06 02:01:02.021973	\N	2026-06-06 01:26:36.842864
6	8	3	3	spam	\N	resolved		2026-06-06 23:33:19.595456	\N	2026-06-06 23:31:01.17083
7	8	2	3	offensive	\N	resolved		2026-06-07 00:01:49.554576	\N	2026-06-06 23:59:16.061561
32	5	12	3	spam		resolved		2026-06-07 03:00:10.403072	\N	2026-06-07 02:59:37.426024
8	5	4	3	offensive		resolved		2026-06-07 00:18:19.39197	\N	2026-06-07 00:17:59.298514
10	5	5	3	spam		resolved		2026-06-07 00:20:27.208493	\N	2026-06-07 00:20:20.779847
9	5	6	3	spam		resolved		2026-06-07 00:20:43.778906	\N	2026-06-07 00:20:16.081584
12	5	7	3	spam		resolved		2026-06-07 01:58:38.929234	\N	2026-06-07 01:57:50.082779
13	5	8	3	spam		resolved		2026-06-07 02:12:28.875442	\N	2026-06-07 02:00:15.243699
15	5	9	3	fake		resolved		2026-06-07 02:17:48.21737	\N	2026-06-07 02:12:09.012064
36	5	12	3	other		resolved		2026-06-07 03:05:59.389708	\N	2026-06-07 03:05:38.029234
14	5	9	3	spam		resolved		2026-06-07 02:18:18.118108	\N	2026-06-07 02:11:20.898105
16	5	9	3	spam		resolved		2026-06-07 02:27:47.27676	\N	2026-06-07 02:27:37.484324
35	5	12	3	fake		resolved		2026-06-07 03:06:40.410698	\N	2026-06-07 03:05:30.053305
17	5	9	3	spam		resolved		2026-06-07 02:28:37.058903	\N	2026-06-07 02:28:27.445787
18	5	9	3	spam		resolved		2026-06-07 02:32:17.429683	\N	2026-06-07 02:31:58.892909
34	5	12	3	offensive		resolved		2026-06-07 03:07:06.790121	\N	2026-06-07 03:05:24.8704
19	5	9	3	spam		resolved		2026-06-07 02:36:44.909703	\N	2026-06-07 02:36:25.394175
20	5	9	3	spam		resolved		2026-06-07 02:40:48.247163	\N	2026-06-07 02:40:38.479664
33	5	12	3	spam		resolved		2026-06-07 03:08:18.609175	\N	2026-06-07 03:05:18.708818
21	5	10	3	spam		resolved		2026-06-07 02:42:22.934865	\N	2026-06-07 02:42:09.948736
22	5	10	3	spam		resolved		2026-06-07 02:45:02.564369	\N	2026-06-07 02:44:30.126636
23	5	11	3	spam		resolved		2026-06-07 02:48:20.96201	\N	2026-06-07 02:48:08.140746
37	5	12	3	spam		resolved		2026-06-07 03:21:57.020102	\N	2026-06-07 03:21:30.10483
24	5	12	3	spam		resolved		2026-06-07 02:49:24.731296	\N	2026-06-07 02:49:16.501668
25	5	12	3	spam		resolved		2026-06-07 02:51:03.05439	\N	2026-06-07 02:50:52.416076
26	5	12	3	spam		resolved		2026-06-07 02:51:53.793987	\N	2026-06-07 02:51:41.103998
27	5	12	3	spam		resolved		2026-06-07 02:52:11.821795	\N	2026-06-07 02:52:06.233041
31	5	12	3	other		resolved		2026-06-07 02:53:44.65775	\N	2026-06-07 02:53:04.550425
40	5	14	3	fake		resolved		2026-06-07 03:37:55.415854	\N	2026-06-07 03:37:25.725202
30	5	12	3	fake		resolved		2026-06-07 02:54:20.945068	\N	2026-06-07 02:53:00.110297
29	5	12	3	offensive		resolved		2026-06-07 02:55:01.80207	\N	2026-06-07 02:52:50.990801
39	5	14	3	offensive		resolved		2026-06-07 03:37:59.924159	\N	2026-06-07 03:37:21.615747
28	5	12	3	spam		resolved		2026-06-07 02:55:32.602514	\N	2026-06-07 02:52:41.782085
38	5	14	3	spam		resolved		2026-06-07 03:38:09.20303	\N	2026-06-07 03:37:17.378611
41	5	14	3	spam		resolved		2026-06-07 03:38:43.380994	\N	2026-06-07 03:38:34.32203
11	5	7	2	spam		rejected		2026-06-11 16:07:12.598261	\N	2026-06-07 00:29:11.35432
5	5	4	2	offensive	\N	under_review		2026-06-11 16:07:20.163639	\N	2026-06-06 12:27:06.660274
\.


--
-- Data for Name: lesson_reviews; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.lesson_reviews (lesson_review_id, enrollment_id, lesson_id, rating, comment, lesson_review_status, created_at, updated_at, is_removed) FROM stdin;
1	2	1	4.00	very nice	ok	2026-06-06 01:17:47.558264	2026-06-06 01:17:47.558289	f
2	2	1	2.00	nhu loz	ok	2026-06-06 01:17:58.381121	2026-06-06 01:17:58.381123	f
3	2	1	5.00	thiet me deo hieu	ok	2026-06-06 01:18:19.549424	2026-06-06 01:18:19.549425	f
4	4	1	2.00	ádasdasd	violating	2026-06-06 12:26:39.031466	2026-06-07 00:18:19.39427	t
5	4	1	5.00	zxcxc	violating	2026-06-07 00:20:06.465087	2026-06-07 00:20:27.209911	t
6	4	1	2.00	asdsad	violating	2026-06-07 00:20:09.403648	2026-06-07 00:20:43.780525	t
7	5	2	5.00	asd	violating	2026-06-07 00:28:29.116395	2026-06-07 01:58:38.930978	t
8	5	2	5.00	asdasd	violating	2026-06-07 01:59:54.015189	2026-06-07 02:12:28.877139	t
9	5	2	5.00	ádasdad	violating	2026-06-07 02:11:01.121297	2026-06-07 02:40:48.252267	t
10	5	2	4.00	asdasd	violating	2026-06-07 02:27:29.040442	2026-06-07 02:45:02.565754	t
11	5	2	4.00	asdasd	violating	2026-06-07 02:47:51.898519	2026-06-07 02:48:20.964345	t
13	5	2	4.00	ádasd	ok	2026-06-07 03:20:58.943772	2026-06-07 03:20:58.943824	f
12	5	2	3.00	asdasd	violating	2026-06-07 02:49:06.154667	2026-06-07 03:21:57.022542	t
14	7	3	5.00	asdasd	violating	2026-06-07 03:36:57.31542	2026-06-07 03:38:43.382566	t
\.


--
-- Data for Name: lessons; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.lessons (lesson_id, course_id, title, description, thumbnail_url, created_at, updated_at, lesson_status, is_removed) FROM stdin;
1	1	lesson 1	\N	\N	2026-06-06 01:01:53.51305	2026-06-06 01:14:18.905384	active	f
2	2	zzz	\N	\N	2026-06-06 16:27:00.361769	2026-06-06 16:27:00.361801	active	f
3	3	wt	\N	\N	2026-06-07 03:35:31.309832	2026-06-07 05:07:18.339042	active	t
4	3	v	\N	\N	2026-06-07 05:07:46.470032	2026-06-08 11:51:16.258025	active	t
5	3	d	\N	\N	2026-06-07 05:09:50.617736	2026-06-07 05:09:50.617736	active	f
\.


--
-- Data for Name: lockouts; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.lockouts (lockout_id, account_id, lockout_type, lockout_level, lockout_start, lockout_end) FROM stdin;
64	5	review	moderate	2026-06-11 15:55:44.118223	2026-06-18 15:55:44.116077
65	5	instructor	severe	2026-06-11 16:05:23.373808	2026-07-11 16:05:23.368578
\.


--
-- Data for Name: managers; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.managers (manager_id, role, display_name, full_name, phone_number, avatar_url, bio) FROM stdin;
2	admin	Super Administrator	\N	\N	\N	\N
3	staff	Hỗ trợ kỹ thuật	\N	\N	\N	\N
14	staff	kien an	\N		\N	\N
\.


--
-- Data for Name: material_completions; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.material_completions (id, enrollment_id, material_id, completed_at) FROM stdin;
2	2	1	2026-06-06 01:17:31.355277
3	2	2	2026-06-06 01:17:33.772518
4	3	1	2026-06-06 02:51:24.950915
5	3	2	2026-06-06 02:51:26.109132
6	4	1	2026-06-06 12:26:31.322558
7	4	2	2026-06-06 12:26:43.413536
8	5	3	2026-06-07 00:28:22.755768
9	6	3	2026-06-07 00:29:27.946735
10	7	4	2026-06-07 03:36:55.080733
\.


--
-- Data for Name: media_embeddings; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.media_embeddings (media_embedding_id, material_id, media_embedding, created_at) FROM stdin;
1	1	[0.021449158,-0.23512185,0.15733972,-0.19233045,-0.07362696,-0.20595154,-0.091949485,0.8068509,0.23909585,-0.35278392,0.18430431,0.073718555,-0.020175185,-0.03705752,-0.32732108,0.09667036,-0.48039708,0.042667836,0.12816145,0.059469096,0.57489884,0.15575492,-0.1134274,-0.20868488,0.07257212,-0.103198454,-0.20781748,-0.10791346,0.06828876,-0.13101004,0.07348008,0.28840345,-0.08666118,0.18407018,0.23277716,-0.1502511,0.1084461,0.021796849,0.16531596,-1.0727427,-0.45413938,0.031104913,0.033768628,-0.18323678,-0.11712984,1.0411373,-0.04113756,0.172385,0.0018442541,0.097340025,-0.10397681,0.04604214,-0.09541738,0.038722787,-0.09677531,0.04803903,0.078028165,-0.013735838,0.051518142,-0.10688275,-0.88895977,-0.062199496,-0.20843291,-0.046458054,-0.1193197,-0.17537156,-0.42073712,-0.47091955,0.08065976,-0.0733331,0.043290757,-0.043465283,-0.15265211,-0.08584222,-0.08072793,0.062166374,-0.2870105,-0.1359311,0.02291208,-0.22033918,0.23075597,0.06847702,0.3348108,-0.34344,0.19546369,0.000114783645,-0.0040894067,0.027698621,0.32494658,-0.11637701,0.26125756,0.13988324,-8.412619,-0.26359767,0.09975904,0.16130206,0.21555822,0.16486263,-0.23906192,-0.24289727,-0.053432863,0.042270996,-0.351677,-0.0879969,-0.08164274,-0.27654642,-1.8897114,-0.10100381,0.09665607,0.23783204,-0.09546184,0.2995147,-0.10336236,-0.17875026,-0.044629827,0.24141972,-0.16276251,0.048870713,0.1556714,0.16993359,0.12722598,0.108922675,0.119504675,0.12525836,0.09333499,-0.04943259,-0.04165948,0.037186846,0.21204615,-0.16759297,-0.18189684,-0.2699734,-0.1861075,1.0146983,-0.022890637,0.28800282,-0.06110801,-0.13148582,-0.0053697135,0.18936442,-0.1253684,-0.42773145,0.37560058,0.14918514,-0.2327598,0.14621177,-0.21744454,0.47793922,0.14337078,-0.24168563,0.39869577,-0.19932641,-0.3072307,-0.06772664,0.08265366,-0.2765007,0.04344252,0.104262255,-0.18969727,-0.09868036,0.04768126,-0.029157598,0.233917,-0.11284427,0.39278585,-0.0288877,0.4853495,0.055350296,0.24439517,0.07380906,0.035985567,-0.061677683,0.14710523,0.017004315,0.022047903,-0.09041697,0.31254262,0.06401594,0.15731505,-0.17165278,0.05202139,-0.068198554,-0.04663905,0.13643149,0.048112262,-0.11420979,0.22049305,0.25668246,0.08640391,0.0051635494,-0.041389916,-0.21270029,-0.123767056,0.09125777,0.14945345,-0.14206173,0.21995842,-0.0012481874,-0.6547929,-0.1890302,0.29307207,0.2875157,-0.07291351,-0.22481528,0.058015525,0.011291004,-0.26138392,0.081961766,-0.010922621,-0.04529511,-0.07331826,0.18852957,0.12531848,0.035035223,0.07480456,-0.08769314,0.11738195,-0.20595627,0.55413413,0.117496744,0.27359125,-0.057658624,0.091176294,0.005114363,0.16829151,0.12530515,0.06338601,0.3718951,-0.23337325,0.14740288,0.08068438,0.05980732,-0.0988227,-0.27345484,0.17890106,-0.067487426,-0.1310645,0.015967008,-0.17068411,-0.058184247,0.1975247,0.20637877,0.14673844,-0.09122711,-0.1307033,0.11842398,-0.031945765,-0.13149649,0.031185448,-0.08206888,-0.18777242,-0.31250075,-0.0050461474,0.08342047,-0.041756254,-0.1905464,-0.31059515,0.050436318,-1.8484478,-0.15358143,0.27113435,0.014381406,-0.04581206,0.5338866,-0.09437895,-0.09820567,-0.062911496,-0.14739507,-0.19374879,-0.08977802,0.22319312,0.30284286,-0.028641647,-0.15499084,-0.20307775,0.14247587,0.08343274,0.12890154,-0.1163352,-0.17973106,-0.08489447,0.21093957,-0.036957107,0.058381964,0.16782546,0.106942534,0.8063569,-0.07286016,-0.077785596,-0.35099226,0.4950944,0.10765751,0.12121149,0.14332691,-0.19110622,0.4001357,-0.30036837,0.20930693,-0.081677414,-0.5053858,0.24874315,-0.48204383,-0.45462123,-0.28234175,0.1217256,-0.897433,-0.15327778,0.09409559,-0.31049013,-0.2230657,-0.09632607,0.33728135,1.0141124,0.18946661,-0.005735067,0.08287572,0.24395058,0.27770075,-0.062165476,0.7410363,-0.03471591,0.6464986,-0.10932882,-0.07475416,-0.080866165,0.102460064,0.0012449473,0.014749566,0.065049306,0.43031326,-0.2903995,0.06599796,0.17201853,-0.4650352,-0.18083742,-0.113616586,-0.14948449,0.24423501,0.32351893,0.0061216084,0.034217026,-0.17836,-0.2970423,0.1482515,-0.142306,0.14097917,-0.027344555,-0.064576015,0.14059967,0.059755523,-0.2660113,0.07091564,0.010707118,0.20525166,0.33955586,-0.3758213,0.18492627,0.2666258,0.09957876,-0.18851212,0.259645,-0.04837451,-0.023856938,0.7882428,-0.7078953,-0.12845606,-0.26380977,-0.13100144,0.20215233,0.28560436,0.14128843,0.18812053,-0.2216065,-0.1077501,0.14786085,-0.085511826,-0.32838935,0.22317012,-0.73318905,0.09310722,-0.27813262,-0.37744164,-0.2605549,-0.26061875,0.10127,-0.34674546,0.21657717,-0.09074117,0.0059115747,0.119027056,-0.013955526,0.07135247,-0.26257712,0.22861798,-0.14223155,-0.02599905,0.17892642,0.16276631,-0.36895937,0.06079154,-0.07407643,0.10934013,0.10743401,-0.41938934,-0.12846081,-0.29166067,-0.17803745,0.15628502,0.386656,-0.3580837,-0.00093554705,0.06941259,-0.0048821345,0.1861224,0.061994243,0.14514334,-0.26961344,0.25644082,-0.08683537,-0.14449657,-0.099437125,-0.24476294,0.13263191,-0.3557875,0.1386131,-0.061696626,-0.027371565,-0.18620214,-1.5717618,-0.09427335,-0.13086788,-0.16022901,-1.1084144,-0.09223982,-0.18500826,0.12845156,0.04194544,-0.19327866,-0.07658818,0.0563355,-0.18454674,0.1541776,-0.03710825,0.16996425,-0.015930219,-0.09957341,0.15907022,0.31249183,0.13854718,-0.18355665,-0.437229,-0.08716916,0.15407756,0.107801296,-0.25582957,0.5939665,0.010761538,-0.41181272,0.1828317,0.026191507,0.043705873,-0.10530722,-0.32755578,0.12364928,0.22384755,0.0124610085,0.8040136,-0.20447466,0.11568527,-0.08761798,0.08954928,0.17763956,0.014632156,0.036430627,-0.2171725,-0.37473902,-0.0014281832,-0.005225537,0.3004948,0.23715277,0.088669375,-0.43604827,0.17612341,0.13553637,0.2806301,0.44849476,-0.15062842,-0.15305102,-0.2007147,-0.14120081,0.30323422,-0.13983396,-0.13518777,0.5256872,-0.17272851,-0.07182307,0.22802344,-0.0793469,-0.20170331,0.02325968,-0.038820855,-0.0010637424,0.1810461,0.07061341,-0.21567735,-0.116962984,-0.39505473,0.04672417,0.049179316,0.5568484,-0.048032247,-0.2301486]	2026-06-06 16:24:03.630128
2	4	[0.29271317,-0.13483909,-0.060201835,-0.5252194,0.16827594,-0.5513569,0.017157936,-0.08794589,0.6874901,-0.09065719,0.44058594,-0.2303042,0.2482971,0.027101984,0.01678512,0.0027698204,-0.26936412,0.006277679,0.22831944,-0.1035432,0.915202,-0.066485055,-0.101292826,0.4872752,-0.605099,0.08284552,0.09695926,-0.08849805,0.019166023,0.13205346,-0.01707624,-0.2838146,-0.17256624,-0.4805339,-0.42388326,0.6272925,0.1593387,0.27456662,0.32175213,0.8045756,-0.16705057,0.60199594,-0.052988183,0.24955142,-0.039198305,2.1101115,0.5884787,-0.23745072,0.6309451,-0.08640786,0.06107239,0.1456798,0.098939344,-0.055717994,-0.17312296,-0.33306575,0.34342685,0.2448521,0.11933337,-0.51739085,0.042077005,0.10430765,-0.048151284,0.38794714,0.04695355,0.09892,-0.33552966,0.3035109,0.023630917,0.06903632,0.62251514,0.2003926,0.2949009,-0.25706643,-0.062001873,-0.23796979,-0.3575052,-0.17334133,0.051532406,0.179106,0.16390707,-0.28169185,-0.11582567,0.23355684,0.3365867,0.30124256,0.251232,-0.08606689,0.6161374,0.010242695,-0.0410569,-0.18992975,-5.5538483,0.7658504,-0.09379925,0.19789848,-0.39675504,-0.060114842,0.20533204,0.060429573,0.40707174,-0.09081869,-0.05239771,0.26198635,-0.29995707,0.21433942,-0.79810154,-0.04498632,-0.08837169,-0.10605005,-0.033293925,0.52648103,0.066703245,-0.01729002,0.46945462,-0.24293527,-0.3796091,0.107136086,0.26191753,-0.30441758,0.46900874,-0.33333033,0.105544664,-0.3494335,0.15100452,-0.6966325,-0.15290612,0.26678318,0.0656053,0.069354706,0.20316815,-0.31974354,0.35840034,0.7914034,0.25550956,0.16020255,-0.07981863,-0.18230931,-0.3081464,-0.10461886,-0.33945295,-0.16786572,0.50270724,0.0067684636,0.20203963,-0.55948,-0.041944608,0.3585207,0.2955246,0.2538405,-0.037821643,-0.15618864,0.7244303,0.5087646,0.12758848,-0.47439104,0.17554763,-0.40365085,-0.41574883,-0.17901437,-0.0827785,0.26364082,-0.04788057,0.4165601,0.19953978,0.083498515,0.9820096,0.09967245,-0.26958713,0.28618956,-0.26744208,0.62040937,0.056088824,-0.3226309,-0.4676959,-0.068242885,-1.2035034,-0.1556002,0.055674307,0.3984267,0.17737529,0.069563404,-0.123297706,-0.102830924,-0.20973197,-0.34219426,0.25066358,0.19232693,-0.03598245,0.24033311,0.07253835,-0.14561284,0.5235933,0.10557729,-0.58807456,0.23833671,-0.3801689,-0.07526759,-0.12071153,0.48279542,-0.28019112,-0.09309557,-0.37148863,0.41761428,-0.035610188,0.2559882,-0.2940246,-0.071381666,0.0066829445,0.087388024,0.38392663,0.27635965,0.47751555,-0.04195881,0.3722883,0.20158,0.42953688,0.3985408,-0.9290695,-0.19321604,0.07784135,-0.37744388,0.36964148,0.07811075,0.13893433,0.11729045,-0.09468427,-0.19673637,-0.022823378,-0.0925733,-0.17331417,-0.08272873,0.071669266,-0.13495837,0.05766452,0.47209448,-0.05860753,-0.16707246,-0.18852611,-0.32732803,0.14895289,0.033032633,0.19402933,-0.44975933,0.041259646,0.48326117,-0.17131248,-0.5974191,0.15375774,0.08546307,0.37688187,-0.09083972,-0.03213453,0.09113528,-0.31126982,-0.016030159,0.029447746,0.20190997,0.28751615,-0.45089078,-0.4195679,-0.28268626,0.24753885,1.3636882,-0.151094,-0.3143743,0.07065757,-0.19420072,0.1869815,0.12594984,-0.08773817,0.45940065,0.09127869,0.3868982,-0.25018284,-0.14837815,0.02521486,-0.19161102,0.001874473,-0.26275718,-0.40346518,0.24892955,0.14204554,-0.01415726,0.09109944,0.067155614,1.1855683,-0.30767882,-0.45635304,-0.1709592,-0.01001361,0.33279115,-0.11735506,-0.1978911,-0.0680794,0.16254368,-0.2687068,0.47117543,-0.0738156,-0.07141539,0.29743388,0.37066156,0.16534671,0.1589661,-0.09304537,-0.3884642,-0.33411497,0.08137803,0.21418051,0.24366148,-0.04450674,-0.07692535,0.7901695,-0.23765573,0.38358727,0.18266626,0.18891111,0.004497237,-0.17082271,0.96214277,0.30822104,1.1806608,-0.21023433,0.29959065,-0.3969366,0.02510348,0.1063277,-0.10406139,-0.12698594,-0.6549061,-0.19569018,-0.110840775,0.31720775,0.14473829,0.06967412,-0.18522756,0.19555834,0.08859793,0.29287568,0.0801812,-0.5819079,-0.24171118,0.30756578,-0.13426815,0.0108738495,0.21978693,-0.07375777,-0.060605098,-0.025334379,0.1395865,-0.77537274,0.031601235,0.36242428,0.5711762,-0.032796256,-1.043296,-0.008867193,0.7527685,-0.034536816,0.13574989,-0.14509186,0.11820271,0.25066152,0.047501065,0.53283256,0.10300642,-0.06983671,-0.32720596,0.08375401,0.2699868,-0.33029658,-0.0658631,0.25152582,0.121713676,-0.0669482,-0.016576342,0.30498767,0.14723158,0.8254484,0.43258157,0.21763845,0.8170969,-0.05186961,-0.2212708,0.33075845,0.12368544,0.14281619,0.15615875,0.172106,-0.7393555,-0.04067751,0.31205612,-0.18146488,0.08545066,0.24952476,-0.27533203,-0.43063992,-0.23055768,-0.3159985,0.0664477,0.6021881,0.15788081,0.74015534,0.3877121,0.18737347,-0.04327416,0.44404596,-0.37056795,0.09968704,-0.17751078,-0.5191001,0.47929066,0.09642463,0.36285657,-0.081417456,-0.16739044,0.011377441,-0.3988355,-0.42418236,0.309978,0.10195211,-0.3653161,0.0721743,0.07853885,-0.40086317,0.016893849,0.25540227,0.1015384,-0.678113,-0.20598741,-0.35853758,-0.09622666,-1.5304171,-0.003229273,0.18318245,0.05874483,-0.57648736,0.10698135,-0.221879,0.048099436,-0.0011339023,0.7633366,-0.37121361,0.5995034,0.60442746,-0.19054726,-0.20666912,-0.011420794,-0.3957648,0.18355846,0.2178065,0.21203914,-0.3216879,1.2483006,0.21886513,-0.51788384,0.6313387,-0.3201056,-0.06559617,-0.11209102,0.21840246,-0.10258883,-0.016940095,-0.05479952,0.4713738,6.10441e-05,0.5427717,0.4077983,0.3122136,-0.395329,-0.353123,0.21964891,0.2153143,-0.012221234,0.17015523,-0.32509902,-0.28954357,0.16951083,-0.065307364,0.023217179,-0.640129,-1.4224474,-0.34353065,0.0064241486,0.17915015,-0.08271812,-0.06891753,0.31845754,0.057827763,-0.061101995,0.015712049,0.076413095,0.047193624,0.53313565,-0.42552742,-0.16400535,-0.026119683,0.03128638,-0.3074655,0.25164098,0.1448624,0.089820996,-0.14664099,-0.009593466,-0.11123125,-0.17134587,-0.15411267,0.3346569,-0.08673772,-0.23404856,0.13132372,0.13802949]	2026-06-07 03:36:17.248881
3	7	[0.10574565,0.002247282,0.28738436,-0.25858563,0.34572467,-0.13788936,0.2688127,0.5143424,0.6150974,0.17394708,0.40502992,-0.183364,0.27280235,-0.26609498,-0.20303942,-0.2287882,-0.9308262,0.5004269,0.08292583,-0.43986514,0.74590427,0.085041605,0.033231888,0.78754526,-0.42732626,0.25549898,0.16169967,-0.33237916,0.078249715,-0.090747766,-0.36084554,-0.15454607,-0.29568836,-0.5670089,0.7339086,0.050848927,0.10996114,-0.07689984,-0.07510368,1.3319069,-0.13844737,0.07977034,-0.12670806,0.20262542,-0.047832035,-0.12024951,0.4099469,0.19973241,0.09818944,0.14214496,0.40289748,0.17440803,-0.14564352,-0.04910786,0.25696227,0.041314933,-0.015859123,0.10426791,-0.007872593,-0.36143953,-0.8446557,0.27125105,0.03664835,0.33715612,0.0295949,-0.116447665,-0.3576189,-0.27109206,-0.050199915,-0.14122294,-0.16851352,0.10202789,-0.18152769,0.08248332,-0.018941045,-0.015565527,-0.09020105,0.07631631,-0.0374046,-0.29511514,0.44836584,-0.073917285,-0.06593583,-0.1243093,0.12399354,0.02364291,0.6681826,0.19401306,0.6655757,-0.06565215,-0.09291399,-0.10187184,-6.4273157,1.0914994,-0.44100744,0.30648026,-0.332436,0.26506966,0.45063195,-0.82865065,0.50713223,-0.33229554,0.34867346,-0.32207474,0.1360016,-0.116188146,-1.0387776,0.34105244,0.0020962756,-0.3410567,-0.14851077,-0.36421376,-0.015825454,-0.15575938,0.18237726,0.022718498,-0.46777073,0.14489225,0.15306695,-0.18282808,-0.03131726,0.50445724,0.17089263,-0.38345113,-0.012914044,-0.22931251,-0.2064632,0.04282895,-0.11714235,-0.08993934,-0.15797092,0.01148901,-0.02475167,0.86803937,0.43092346,-0.023959825,-0.19422683,-0.05330689,-0.18459326,0.065269046,0.23525034,0.18756247,0.2993404,0.008453156,0.13089597,-0.12374581,-0.1825547,0.7030576,-0.06898327,-0.23258774,-0.28501862,-0.20782031,0.7649037,0.05585626,0.02037254,-0.6544913,0.06340031,-0.21902509,-0.25209695,-0.13380714,0.05845425,-0.30912554,0.18331847,0.22722961,0.046997223,-0.16221961,0.23977295,-0.11254708,-0.27216062,-0.3250718,0.42139006,0.13233538,-0.10505391,0.03690999,-0.35627916,0.177184,-0.8569363,-0.051385917,0.14194722,0.4735717,0.081035055,-0.27105695,-0.13490824,-0.12261463,-0.15830085,0.011926695,-0.11965932,0.27444208,-0.11746852,0.11727322,0.040066287,-0.04847232,0.1767467,-0.23520902,0.068581104,0.1852642,-0.077379145,0.07581759,-0.5498831,0.20863934,-0.2022176,0.011403938,-0.5532439,0.24617496,-0.06735219,0.111038014,0.010844311,0.19944191,-0.14546967,-0.28683642,-0.26859903,0.17096154,0.19407396,0.0039681303,0.29083142,0.31229928,0.42107472,-0.0030878494,-0.36165333,-0.19721805,0.859318,-0.2330639,0.048266266,-0.108595945,0.01326037,-0.016614087,-0.028316833,-0.06043279,0.15778677,0.16792837,-0.6715788,-0.25685367,0.12739573,-0.20162739,0.37291276,0.5097392,-0.20677386,0.0014393697,-0.15704402,-0.4810707,-0.24481727,0.018628439,0.06782404,-0.27851173,-0.037082985,0.5164655,0.114049315,-0.23553346,-0.13325885,-0.06507954,-0.09299628,-0.098424435,0.25060937,-0.19579954,-0.2949848,0.098636165,-0.41602358,0.11326362,0.68698806,-0.022581927,0.07073383,-0.17209077,0.25589058,0.5912549,-0.20690142,0.05636748,0.58360386,-0.36149347,-0.0639405,0.19468026,-0.07263391,-0.2142169,-0.16622303,-0.054831564,-0.47719273,-0.107147,-0.073897816,-0.25578922,0.24244134,0.05921968,-0.31471917,0.12176202,0.1815284,-0.10441565,-0.12275017,-0.055825803,-0.21618257,0.019524671,-0.4606452,-0.19022462,-0.046445396,0.30584857,0.10090466,0.076905444,0.060046695,-0.008452465,-0.6525717,0.6771649,-0.27826765,-0.34142792,0.07823043,0.48854542,-0.10266569,-0.37979373,0.15666558,-0.6037991,-0.11916117,0.37598714,0.13361345,0.24025919,-0.00026491395,-0.09124143,0.868059,0.041505102,0.44699028,0.19733648,0.04017024,-0.40715346,-0.22856887,0.87743706,-0.0059959083,-0.10136255,-0.08076338,-0.04674445,-0.39745638,0.58411634,-0.09230871,-0.3139895,-0.0831027,0.034072224,-0.22670563,0.04770429,0.46610153,0.15953642,-0.22556743,-0.034383815,0.322538,0.2374913,-0.030968111,0.30137423,-0.1974774,-0.5836986,0.3331543,0.17873761,-0.09543882,-0.006973562,-0.072398804,0.22481395,-0.20468982,-0.35687444,-0.3174444,0.09248826,-0.11315275,0.35807344,-0.14647166,0.23134676,-0.38646132,0.9223109,-0.25881037,-0.078309454,-0.2111759,-0.03887892,0.5033371,-0.1865851,0.014844899,0.07893657,-0.41419193,-0.28740337,0.066943385,0.13427322,-0.08214841,-0.040509462,0.29665565,-0.07897584,-0.20106667,0.16361633,-0.6342554,0.11426802,0.8667033,0.52561617,0.10275777,0.42627022,-0.14095744,-0.32292277,0.045839507,-0.20479226,-0.12556347,-0.07570229,-0.15771368,-0.31467015,0.3414233,0.12725133,-0.34093684,-0.14793915,0.1479489,-0.026100783,-0.05116183,0.16818473,0.44652236,-0.2204964,0.3814802,0.32982537,0.22996828,0.16150482,-0.13837382,0.1187786,0.06174531,-0.44189167,0.040163476,-0.14577225,-0.18465945,0.20639557,-0.06394103,0.26068786,-0.0066264006,0.24161814,0.09602185,-0.6018856,-0.1256431,-0.029697571,-0.06805028,-0.37467095,0.15271387,-0.32114327,-0.49744645,0.092757806,-0.071838096,0.0113955205,-0.5002796,0.18087451,-0.5396445,-0.1820963,-0.8949645,0.4205122,0.008541848,-0.2805735,0.25116637,-0.0030598952,-0.38221985,0.11244219,-0.02296501,0.7745325,-0.5106215,0.508834,0.6567057,-0.13749717,-0.17663977,-0.11227989,0.20895292,-0.34267834,-0.076092884,0.129194,-0.3193437,0.50562507,0.22174354,0.23413244,0.3453215,0.0024913454,-0.0043278243,0.06645541,-0.05571346,0.31340188,-0.028634693,0.27350596,0.15160456,0.084891796,0.28274372,0.30523545,0.28316325,0.21491256,0.0036010058,-0.26879779,-0.20754728,-0.24618998,0.27509636,-0.2795472,0.3472251,0.2976968,-0.016200775,-0.2980613,-0.6657658,-0.84125346,-0.13262224,-0.05701082,0.4814665,0.39082953,0.00022041524,0.43045792,-0.061580427,-0.015007472,-0.112035714,-0.33715,0.5466419,0.11293047,-0.001499072,-0.30458304,-0.1623871,-0.22061019,-0.19571239,0.40210575,-0.04404464,-0.13352886,-0.16364227,-0.25995383,0.6270939,-0.085767366,0.08568617,0.39974153,-0.23567937,0.16306223,0.07008547,-0.0703621]	2026-06-08 11:51:40.832524
\.


--
-- Data for Name: message_attachments; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.message_attachments (attachment_id, message_id, file_url, file_name, file_type, file_size, created_at) FROM stdin;
\.


--
-- Data for Name: message_moderation_logs; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.message_moderation_logs (log_id, model_id, message_id, input_json, output_json, latency_ms, error_message, log_created_at) FROM stdin;
\.


--
-- Data for Name: messages; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.messages (message_id, chat_id, sender_id, content, is_seen, message_status, sent_at, received_at) FROM stdin;
2	2	5	e ku	f	ok	2026-06-06 23:17:27.330092	\N
3	3	8	ê ku	f	ok	2026-06-06 23:17:35.229551	\N
4	4	9	ê cụ ơi	f	ok	2026-06-06 23:17:44.261135	\N
\.


--
-- Data for Name: notifications; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.notifications (notification_id, sender_id, receiver_id, title, content, link_action, is_read, is_removed, created_at) FROM stdin;
1	\N	1	Application Result	Congratulations! Your instructor application has been approved.	/InstructorCourse/Create	f	f	2026-06-06 07:59:28.878953
3	\N	4	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 08:16:52.734076
5	\N	1	New Review	Your course 'khoa hoc 1' just received a 2-star review from a student.	/InstructorCourse/Editor?id=1	t	f	2026-06-06 08:17:58.385595
6	\N	1	New Review	Your course 'khoa hoc 1' just received a 5-star review from a student.	/InstructorCourse/Editor?id=1	t	f	2026-06-06 08:18:19.554766
4	\N	1	New Review	Your course 'khoa hoc 1' just received a 4-star review from a student.	/InstructorCourse/Editor?id=1	t	f	2026-06-06 08:17:47.610156
2	\N	1	Course Approved	Your course 'khoa hoc 1' has been approved and is now published in the store. 	/Course/Details/1	t	f	2026-06-06 08:16:52.704935
8	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	\N	t	f	2026-06-06 08:21:23.123355
7	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	\N	t	f	2026-06-06 08:21:18.586197
9	\N	1	Report Resolution Update	Your report about the lesson review has been accepted.	\N	t	f	2026-06-06 08:22:46.618722
10	\N	1	New Review	Your course 'khoa hoc 1' just received a 4-star review from a student.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 08:35:05.368748
12	\N	1	Report Resolution Update	Your report about the course review has been accepted.	\N	f	f	2026-06-06 08:36:16.7947
11	\N	5	Community Standards Violation (1st Warning)	Your comment has been removed for violating community standards. This is your first warning.	\N	t	f	2026-06-06 08:36:16.781112
13	\N	5	Commenting Restricted (2nd Violation)	Due to repeated violations, you are restricted from posting comments or reviews for 7 days.	\N	f	f	2026-06-06 08:38:02.139768
14	\N	5	Report Resolution Update	Your report about the course review has been accepted.	\N	t	f	2026-06-06 08:38:02.16104
15	\N	1	New Review	Your course 'khoa hoc 1' just received a 4-star review from a student.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 08:39:41.437157
16	\N	5	Community Standards Violation (1st Warning)	Your comment has been removed for violating community standards. This is your first warning.	\N	f	f	2026-06-06 08:40:09.516345
17	\N	1	Report Resolution Update	Your report about the course review has been accepted.	\N	f	f	2026-06-06 08:40:09.528378
18	\N	1	Report Resolution Update	Your report about the course review has been accepted.	\N	f	f	2026-06-06 08:40:11.874679
19	\N	1	Report Resolution Update	Your report about the lesson review has been reviewed and dismissed.	\N	f	f	2026-06-06 09:01:02.025986
20	\N	1	New Review	Your course 'khoa hoc 1' just received a 4-star review from a student.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 09:27:34.930431
21	\N	1	Course Violation Reminder (1st Time)	Your course 'khoa hoc 1' has received a policy violation warning and has been rejected. Reason: . Please review and correct the content.	/instructor/courses	f	f	2026-06-06 09:38:29.331094
22	\N	7	Report Resolution Update	Your report has been accepted and the violating content has been actioned.	/courses/1	f	f	2026-06-06 09:38:29.394973
23	\N	1	Severe Violation Warning (2nd Time)	Your course 'khoa hoc 1' has been flagged for violation (2nd time) and rejected from Marketplace. Reason: . Please comply with platform policies.	/instructor/courses	f	f	2026-06-06 09:50:20.034522
24	\N	7	Report Resolution Update	Your report has been accepted and the violating content has been actioned.	/courses/1	f	f	2026-06-06 09:50:20.05195
25	\N	4	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-06 09:50:30.076087
26	\N	5	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-06 09:50:30.103516
27	\N	7	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-06 09:50:30.107558
29	\N	7	Report Resolution Update	Your report has been accepted and the violating content has been actioned.	/courses/1	f	f	2026-06-06 09:50:30.11834
28	\N	1	Permanent Course Discontinuation Notice (3rd Time)	Your course 'khoa hoc 1' has violated our policies for the 3rd time. It has been permanently discontinued and archived. New enrollments are disabled.	/instructor/courses	t	f	2026-06-06 09:50:30.111474
30	\N	8	Report Resolution Update	Your report about the course review has been reviewed and dismissed.	\N	f	f	2026-06-06 11:04:44.966749
32	\N	5	Review Policy Warning	Your course review has received a policy violation warning. Reason: . Please ensure your content complies with our guidelines.	/Course/Details/1#reviews	f	f	2026-06-06 14:25:34.164099
33	\N	5	Review Policy Warning	Your course review has received a policy violation warning. Reason: . Please ensure your content complies with our guidelines.	/Course/Details/1#reviews	t	f	2026-06-06 14:25:58.60245
35	\N	8	Report Resolution Update	Your report about the course review has been accepted.	\N	f	f	2026-06-06 14:40:15.130892
34	\N	5	Review Policy Warning	Your course review has received a policy violation warning. Reason: . Please ensure your content complies with our guidelines.	/Course/Details/1#reviews	t	f	2026-06-06 14:40:14.948001
37	\N	8	Report Resolution Update	Your report about the course review has been accepted.	\N	f	f	2026-06-06 14:41:01.248242
36	\N	5	Commenting Restricted (2nd Violation)	Due to repeated violations, you are restricted from posting comments or reviews for 7 days.	\N	t	f	2026-06-06 14:41:01.199805
39	\N	8	Report Resolution Update	Your report about the course review has been accepted.	\N	f	f	2026-06-06 14:42:04.773073
38	\N	5	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	\N	t	f	2026-06-06 14:42:04.743047
31	\N	1	New Review	Your course 'khoa hoc 1' just received a 3-star review from a student.	/Course/Details/1#review-card-4	t	f	2026-06-06 14:24:32.194352
41	\N	4	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 14:47:23.993609
42	\N	5	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 14:47:23.993609
43	\N	7	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 14:47:23.993609
44	\N	8	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 14:47:23.993609
47	\N	4	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-06 19:06:14.099152
49	\N	7	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-06 19:06:14.11044
50	\N	8	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-06 19:06:14.113182
46	\N	5	Report Resolution Update	Your report has been accepted and appropriate action has been taken.	/Course/Details/1	t	f	2026-06-06 19:06:02.618084
48	\N	5	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	t	f	2026-06-06 19:06:14.106918
51	\N	1	Permanent Course Discontinuation Notice	Your course 'khoa hoc 1' has violated our policies. It has been permanently discontinued and archived (Strike 3). New enrollments are disabled. Furthermore, your instructor account is locked for 30 days.	/Course/Details/1	t	f	2026-06-06 19:06:14.115772
40	\N	1	Course Approved	Your course 'khoa hoc 1' has been approved and is now published in the store. 	/Course/Details/1	t	f	2026-06-06 14:47:23.966293
45	\N	1	Course Violation Warning	Your course 'khoa hoc 1' has received a policy violation warning. This is Strike 2. Reason: . Please review and correct the content.	/InstructorCourse/Editor/1	t	f	2026-06-06 19:06:02.603027
52	\N	5	Report Resolution Update	Your report has been accepted and appropriate action has been taken.	/Course/Details/1	t	f	2026-06-06 19:06:14.1231
53	\N	5	Report Resolution Update	Your report is currently under review.	/Course/Details/1	t	f	2026-06-06 19:25:34.004893
54	\N	1	New Review	Your course 'khoa hoc 1' just received a 2-star review from a student.	/Course/Learn/1#review-card-4	f	f	2026-06-06 19:26:39.061202
55	\N	1	New Review	Your course 'khoa hoc 1' just received a 5-star review from a student.	/Course/Details/1#review-card-5	f	f	2026-06-06 19:26:48.11879
56	\N	8	Review Policy Warning	Your course review has received a policy violation warning. Reason: . Please ensure your content complies with our guidelines.	/Course/Details/1#reviews	f	f	2026-06-06 19:27:48.064059
57	\N	5	Report Resolution Update	Your report about the course review has been accepted.	\N	f	f	2026-06-06 19:27:48.073888
58	\N	8	Report Resolution Update	Your report about the course review has been reviewed and dismissed.	\N	f	f	2026-06-06 19:28:13.55462
59	\N	5	Report Resolution Update	Your report about the lesson review is currently under review.	\N	f	f	2026-06-06 19:30:11.714645
60	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 2.	/InstructorCourse/Editor/1	f	f	2026-06-06 19:30:50.208799
61	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 1.	/InstructorCourse/Editor/1	f	f	2026-06-06 19:31:06.063635
62	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 0.	/InstructorCourse/Editor/1	f	f	2026-06-06 19:31:14.797364
63	\N	1	Course Violation Reminder (1st Time)	Your course 'khoa hoc 1' has been flagged for a violation and temporarily hidden. Reason: z. Please review the content and ensure compliance.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 19:36:25.261216
64	\N	1	Severe Violation Warning (2nd Time)	Your course 'khoa hoc 1' continues to violate policies (2nd time). This is a strong warning. If the violation continues, the course will be permanently deleted.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 19:37:00.087743
65	\N	1	Permanent Course Discontinuation Notice (3rd Time)	Your course 'khoa hoc 1' has violated policies for the 3rd time. The platform has decided to permanently discontinue this course. You will not be able to edit the content or accept new students, but existing students can still access their purchased content.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 19:37:10.301931
66	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 2.	/InstructorCourse/Editor/1	f	f	2026-06-06 19:38:15.542112
67	\N	4	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-06 19:39:06.401299
68	\N	5	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-06 19:39:06.409008
69	\N	7	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-06 19:39:06.411106
70	\N	8	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-06 19:39:06.415193
72	\N	5	Report Resolution Update	Your report has been accepted and appropriate action has been taken.	/Course/Details/1	f	f	2026-06-06 19:39:06.42611
73	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 2.	/InstructorCourse/Editor/1	f	f	2026-06-06 19:45:07.471554
108	\N	5	Report Resolution Update	Your report has been accepted and appropriate action has been taken.	/Course/Details/1	f	f	2026-06-06 19:49:55.716429
109	\N	1	Course Violation Warning	Your course 'khoa hoc 1' has received a policy violation warning. This is Strike 2. Reason: . Please review and correct the content.	/InstructorCourse/Editor/1	f	f	2026-06-06 19:49:59.176942
110	\N	5	Report Resolution Update	Your report has been accepted and appropriate action has been taken.	/Course/Details/1	f	f	2026-06-06 19:49:59.18553
111	\N	1	Permanent Course Discontinuation Notice (3rd Time)	Your course 'khoa hoc 1' has violated policies for the 3rd time. The platform has decided to permanently discontinue this course. You will not be able to edit the content or accept new students, but existing students can still access their purchased content.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 19:50:21.368982
112	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 2.	/InstructorCourse/Editor/1	f	f	2026-06-06 20:32:45.300592
71	\N	1	Permanent Course Discontinuation Notice	Your course 'khoa hoc 1' has violated our policies. It has been permanently discontinued and archived (Strike 3). New enrollments are disabled. Furthermore, your instructor account is locked for 30 days.	/Course/Details/1	f	f	2026-06-06 19:39:06.418274
74	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 1.	/InstructorCourse/Editor/1	f	f	2026-06-06 19:45:08.881603
75	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 0.	/InstructorCourse/Editor/1	f	f	2026-06-06 19:45:11.320572
76	\N	1	Course Violation Reminder (1st Time)	Your course 'khoa hoc 1' has been flagged for a violation and temporarily hidden. Reason: j. Please review the content and ensure compliance.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 19:46:26.028606
77	\N	1	Severe Violation Warning (2nd Time)	Your course 'khoa hoc 1' continues to violate policies (2nd time). This is a strong warning. If the violation continues, the course will be permanently deleted.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 19:46:38.426725
78	\N	1	Permanent Course Discontinuation Notice (3rd Time)	Your course 'khoa hoc 1' has violated policies for the 3rd time. The platform has decided to permanently discontinue this course. You will not be able to edit the content or accept new students, but existing students can still access their purchased content.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 19:46:45.652911
79	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 2.	/InstructorCourse/Editor/1	f	f	2026-06-06 19:47:00.383938
80	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 1.	/InstructorCourse/Editor/1	f	f	2026-06-06 19:47:03.154414
81	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 0.	/InstructorCourse/Editor/1	f	f	2026-06-06 19:47:05.619341
82	\N	1	Course Approved	Your course 'khoa hoc 1' has been approved and is now published in the store. 	/Course/Details/1	f	f	2026-06-06 19:47:38.917652
83	\N	4	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 19:47:38.923679
84	\N	5	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 19:47:38.923679
85	\N	7	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 19:47:38.923679
86	\N	8	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 19:47:38.923679
87	\N	1	Course Violation Warning	Your course 'khoa hoc 1' has received a policy violation warning. This is Strike 1. Reason: . Please review and correct the content.	/InstructorCourse/Editor/1	f	f	2026-06-06 19:48:10.291188
88	\N	5	Report Resolution Update	Your report has been accepted and appropriate action has been taken.	/Course/Details/1	f	f	2026-06-06 19:48:10.303204
89	\N	1	Course Violation Warning	Your course 'khoa hoc 1' has received a policy violation warning. This is Strike 2. Reason: . Please review and correct the content.	/InstructorCourse/Editor/1	f	f	2026-06-06 19:48:18.011211
90	\N	5	Report Resolution Update	Your report has been accepted and appropriate action has been taken.	/Course/Details/1	f	f	2026-06-06 19:48:18.020415
91	\N	4	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-06 19:48:27.000276
92	\N	5	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-06 19:48:27.004886
93	\N	7	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-06 19:48:27.007442
94	\N	8	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-06 19:48:27.011289
95	\N	1	Permanent Course Discontinuation Notice	Your course 'khoa hoc 1' has violated our policies. It has been permanently discontinued and archived (Strike 3). New enrollments are disabled. Furthermore, your instructor account is locked for 30 days.	/Course/Details/1	f	f	2026-06-06 19:48:27.014126
96	\N	5	Report Resolution Update	Your report has been accepted and appropriate action has been taken.	/Course/Details/1	f	f	2026-06-06 19:48:27.020024
97	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 2.	/InstructorCourse/Editor/1	f	f	2026-06-06 19:48:50.788746
98	\N	1	Permanent Course Discontinuation Notice (3rd Time)	Your course 'khoa hoc 1' has violated policies for the 3rd time. The platform has decided to permanently discontinue this course. You will not be able to edit the content or accept new students, but existing students can still access their purchased content.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 19:49:12.651772
99	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 2.	/InstructorCourse/Editor/1	f	f	2026-06-06 19:49:25.3478
100	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 1.	/InstructorCourse/Editor/1	f	f	2026-06-06 19:49:27.527765
101	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 0.	/InstructorCourse/Editor/1	f	f	2026-06-06 19:49:30.21659
102	\N	1	Course Approved	Your course 'khoa hoc 1' has been approved and is now published in the store. 	/Course/Details/1	f	f	2026-06-06 19:49:40.297744
103	\N	4	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 19:49:40.30209
104	\N	5	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 19:49:40.30209
105	\N	7	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 19:49:40.30209
106	\N	8	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 19:49:40.30209
107	\N	1	Course Violation Warning	Your course 'khoa hoc 1' has received a policy violation warning. This is Strike 1. Reason: . Please review and correct the content.	/InstructorCourse/Editor/1	f	f	2026-06-06 19:49:55.708476
113	\N	1	Permanent Course Discontinuation Notice (3rd Time)	Your course 'khoa hoc 1' has violated policies for the 3rd time. The platform has decided to permanently discontinue this course. You will not be able to edit the content or accept new students, but existing students can still access their purchased content.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 20:33:40.422197
114	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 2.	/InstructorCourse/Editor/1	f	f	2026-06-06 20:33:52.105347
116	\N	5	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-06 20:35:43.71854
117	\N	7	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-06 20:35:43.725009
120	\N	5	Report Resolution Update	Your report has been accepted and appropriate action has been taken.	/Course/Details/1	f	f	2026-06-06 20:35:43.74135
115	\N	4	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-06 20:35:43.692618
118	\N	8	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-06 20:35:43.728565
119	\N	1	Permanent Course Discontinuation Notice	Your course 'khoa hoc 1' has violated our policies. It has been permanently discontinued and archived (Strike 3). New enrollments are disabled. Furthermore, your instructor account is locked for 30 days.	/Course/Details/1	f	f	2026-06-06 20:35:43.732654
121	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 2.	/InstructorCourse/Editor/1	f	f	2026-06-06 21:00:38.530371
122	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 1.	/InstructorCourse/Editor/1	f	f	2026-06-06 21:32:03.123632
123	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 0.	/InstructorCourse/Editor/1	f	f	2026-06-06 21:32:09.713567
124	\N	1	Course Violation Reminder (1st Time)	Your course 'khoa hoc 1' has been flagged for a violation and temporarily hidden. Reason: z. Please review the content and ensure compliance.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 21:32:44.395693
125	\N	1	Course Rejected	Your course 'khoa hoc 1' was not approved. Please check details in the course editor.	/InstructorCourse/Editor?id=1	t	f	2026-06-06 21:33:30.447704
126	\N	1	Severe Violation Warning (2nd Time)	Your course 'khoa hoc 1' continues to violate policies (2nd time). This is a strong warning. If the violation continues, the course will be permanently deleted.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 21:34:17.287877
127	\N	1	Permanent Course Discontinuation Notice (3rd Time)	Your course 'khoa hoc 1' has violated policies for the 3rd time. The platform has decided to permanently discontinue this course. You will not be able to edit the content or accept new students, but existing students can still access their purchased content.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 21:34:24.145536
128	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 2.	/InstructorCourse/Editor/1	f	f	2026-06-06 21:36:04.060539
129	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 1.	/InstructorCourse/Editor/1	f	f	2026-06-06 21:36:07.038189
130	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 0.	/InstructorCourse/Editor/1	f	f	2026-06-06 21:36:09.507744
131	\N	1	Course Violation Reminder (1st Time)	Your course 'khoa hoc 1' has been flagged for a violation and temporarily hidden. Reason: d. Please review the content and ensure compliance.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 21:36:41.838732
132	\N	1	Course Approved	Your course 'khoa hoc 1' has been approved and is now published in the store. 	/Course/Details/1	f	f	2026-06-06 21:36:58.05168
133	\N	4	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 21:36:58.073972
134	\N	5	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 21:36:58.073972
135	\N	7	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 21:36:58.073972
136	\N	8	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 21:36:58.073972
137	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 0.	/InstructorCourse/Editor/1	f	f	2026-06-06 21:38:07.112977
138	\N	1	Course Violation Reminder (1st Time)	Your course 'khoa hoc 1' has been flagged for a violation and temporarily hidden. Reason: x. Please review the content and ensure compliance.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 21:38:12.045751
139	\N	1	Severe Violation Warning (2nd Time)	Your course 'khoa hoc 1' continues to violate policies (2nd time). This is a strong warning. If the violation continues, the course will be permanently deleted.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 21:38:50.522638
140	\N	1	Permanent Course Discontinuation Notice (3rd Time)	Your course 'khoa hoc 1' has violated policies for the 3rd time. The platform has decided to permanently discontinue this course. You will not be able to edit the content or accept new students, but existing students can still access their purchased content.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 21:39:28.767531
141	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 2.	/InstructorCourse/Editor/1	f	f	2026-06-06 21:39:33.896839
142	\N	1	Permanent Course Discontinuation Notice (3rd Time)	Your course 'khoa hoc 1' has violated policies for the 3rd time. The platform has decided to permanently discontinue this course. You will not be able to edit the content or accept new students, but existing students can still access their purchased content.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 22:05:02.208431
143	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 2.	/InstructorCourse/Editor/1	f	f	2026-06-06 22:10:40.991632
144	\N	1	Permanent Course Discontinuation Notice (3rd Time)	Your course 'khoa hoc 1' has violated policies for the 3rd time. The platform has decided to permanently discontinue this course. You will not be able to edit the content or accept new students, but existing students can still access their purchased content.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 22:10:48.706079
145	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 2.	/InstructorCourse/Editor/1	f	f	2026-06-06 22:10:51.121638
146	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 1.	/InstructorCourse/Editor/1	f	f	2026-06-06 22:10:53.65524
147	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 0.	/InstructorCourse/Editor/1	f	f	2026-06-06 22:10:58.076626
148	\N	1	Course Violation Reminder (1st Time)	Your course 'khoa hoc 1' has been flagged for a violation and temporarily hidden. Reason: v. Please review the content and ensure compliance.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 22:11:13.945612
149	\N	1	Course Approved	Your course 'khoa hoc 1' has been approved and is now published in the store. 	/Course/Details/1	f	f	2026-06-06 22:11:22.921691
150	\N	4	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 22:11:22.948907
155	\N	1	Course Violation Reminder (1st Time)	Your course 'khoa hoc 1' has been flagged for a violation and temporarily hidden. Reason: b. Please review the content and ensure compliance.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 22:12:02.502417
151	\N	5	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 22:11:22.948907
152	\N	7	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 22:11:22.948907
153	\N	8	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 22:11:22.948907
154	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 0.	/InstructorCourse/Editor/1	f	f	2026-06-06 22:11:35.914118
156	\N	1	Course Approved	Your course 'khoa hoc 1' has been approved and is now published in the store. 	/Course/Details/1	f	f	2026-06-06 22:13:03.858425
157	\N	4	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 22:13:03.864857
158	\N	5	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 22:13:03.864857
159	\N	7	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 22:13:03.864857
160	\N	8	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 22:13:03.864857
161	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 0.	/InstructorCourse/Editor/1	f	f	2026-06-06 22:13:10.107148
162	\N	1	Course Rejected	Your course 'khoa hoc 1' was not approved. Please check details in the course editor.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 22:13:18.674433
163	\N	1	Course Violation Reminder (1st Time)	Your course 'khoa hoc 1' has been flagged for a violation and temporarily hidden. Reason: d. Please review the content and ensure compliance.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 22:13:25.077345
164	\N	1	Severe Violation Warning (2nd Time)	Your course 'khoa hoc 1' continues to violate policies (2nd time). This is a strong warning. If the violation continues, the course will be permanently deleted.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 22:13:31.94491
165	\N	1	Permanent Course Discontinuation Notice (3rd Time)	Your course 'khoa hoc 1' has violated policies for the 3rd time. The platform has decided to permanently discontinue this course. You will not be able to edit the content or accept new students, but existing students can still access their purchased content.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 22:13:34.265077
166	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 2.	/InstructorCourse/Editor/1	f	f	2026-06-06 23:23:08.354904
167	\N	2	Manual Audit Required	Course 1 requires manual review following AI Moderation. Threat Level: FlaggedOrRejected.\n\nDetails:\nAI flagged the following issues:\n- [course.what_you_will_learn, course.requirements] Harmful content detected in fields: course.what_you_will_learn, course.requirements	/AdminModeration/Courses	f	f	2026-06-06 23:24:03.771225
168	\N	1	Course Approved	Your course 'khoa hoc 1' has been approved and is now published in the store. 	/Course/Details/1	f	f	2026-06-06 23:25:12.864586
169	\N	4	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 23:25:12.890061
170	\N	5	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 23:25:12.890061
171	\N	7	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 23:25:12.890061
172	\N	8	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 23:25:12.890061
173	\N	2	Manual Audit Required	Course 2 requires manual review following AI Moderation. Threat Level: FlaggedOrRejected.\n\nDetails:\nAI flagged the following issues:\n- [material_3] Found a semantic duplication for material_3 on material_1 (cosine similarity: 1.0 > 0.8)	/AdminModeration/Courses	f	f	2026-06-06 23:27:29.3824
174	\N	1	Permanent Course Discontinuation Notice (3rd Time)	Your course 'khoa hoc 1' has violated policies for the 3rd time. The platform has decided to permanently discontinue this course. You will not be able to edit the content or accept new students, but existing students can still access their purchased content.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 23:28:13.426864
175	\N	1	Course Approved	Your course 'khoa hoc 1' has been approved and is now published in the store. 	/Course/Details/1	f	f	2026-06-06 23:28:20.885661
176	\N	4	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 23:28:20.890675
177	\N	5	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 23:28:20.890675
178	\N	7	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 23:28:20.890675
179	\N	8	Course Content Updated	The course 'khoa hoc 1' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=1	f	f	2026-06-06 23:28:20.890675
180	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 2.	/InstructorCourse/Editor/1	f	f	2026-06-06 23:28:28.978245
181	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 1.	/InstructorCourse/Editor/1	f	f	2026-06-06 23:28:57.333767
182	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 0.	/InstructorCourse/Editor/1	f	f	2026-06-06 23:29:00.540388
183	\N	1	Course Rejected	Your course 'khoa hoc 1' was not approved. Please check details in the course editor.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 23:29:25.836738
184	\N	1	Course Violation Reminder (1st Time)	Your course 'khoa hoc 1' has been flagged for a violation and temporarily hidden. Reason: x. Please review the content and ensure compliance.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 23:29:53.699807
185	\N	1	Severe Violation Warning (2nd Time)	Your course 'khoa hoc 1' continues to violate policies (2nd time). This is a strong warning. If the violation continues, the course will be permanently deleted.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 23:30:05.42056
186	\N	1	Permanent Course Discontinuation Notice (3rd Time)	Your course 'khoa hoc 1' has violated policies for the 3rd time. The platform has decided to permanently discontinue this course. You will not be able to edit the content or accept new students, but existing students can still access their purchased content.	/InstructorCourse/Editor?id=1	f	f	2026-06-06 23:30:07.898012
260	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=2&lessonId=2#review-card-7	f	f	2026-06-07 08:58:38.95231
188	\N	5	Report Resolution Update	Your report about the course review has been accepted.	\N	f	f	2026-06-07 06:32:15.570783
189	\N	5	Review Policy Warning	Your lesson review has received a policy violation warning. Reason: . Please ensure your content complies with our guidelines.	/Course/Learn?id=1&lessonId=1#review-card-3	f	f	2026-06-07 06:33:19.597319
190	\N	8	Report Resolution Update	Your report about the lesson review has been accepted.	\N	f	f	2026-06-07 06:33:19.612138
187	\N	8	Review Policy Warning	Your course review has received a policy violation warning. Reason: . Please ensure your content complies with our guidelines.	/Course/Details/1#review-card-5	t	f	2026-06-07 06:32:15.499656
191	\N	5	Review Policy Warning	Your lesson review has received a policy violation warning. Reason: . Please ensure your content complies with our guidelines.	/Course/Learn?id=1&lessonId=1#review-card-2	f	f	2026-06-07 07:01:49.55782
192	\N	8	Report Resolution Update	Your report about the lesson review has been accepted.	\N	f	f	2026-06-07 07:01:49.632425
193	\N	8	Review Policy Warning	Your course review has received a policy violation warning. Reason: . Please ensure your content complies with our guidelines.	/Course/Details/1#review-card-5	f	f	2026-06-07 07:01:52.781298
194	\N	5	Report Resolution Update	Your report about the course review has been accepted.	\N	f	f	2026-06-07 07:01:52.792663
195	\N	8	Community Standards Violation (1st Warning)	Your comment has been removed for violating community standards. This is your first warning.	/Course/Learn?id=1&lessonId=1#review-card-4	f	f	2026-06-07 07:18:19.412915
196	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	\N	t	f	2026-06-07 07:18:19.525573
197	\N	1	New Review	Your course 'khoa hoc 1' just received a 4-star review from a student.	/Course/Details/1#review-card-6	f	f	2026-06-07 07:19:50.27509
198	\N	1	New Review	Your course 'khoa hoc 1' just received a 3-star review from a student.	/Course/Details/1#review-card-7	f	f	2026-06-07 07:19:53.011718
199	\N	1	New Review	Your course 'khoa hoc 1' just received a 5-star review from a student.	/Course/Learn/1#review-card-5	f	f	2026-06-07 07:20:06.48052
200	\N	1	New Review	Your course 'khoa hoc 1' just received a 2-star review from a student.	/Course/Learn/1#review-card-6	f	f	2026-06-07 07:20:09.409041
201	\N	8	Commenting Restricted (2nd Violation)	Due to repeated violations, you are restricted from posting comments or reviews for 7 days.	/Course/Learn?id=1&lessonId=1#review-card-5	f	f	2026-06-07 07:20:27.228835
202	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	\N	f	f	2026-06-07 07:20:27.251876
203	\N	8	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Learn?id=1&lessonId=1#review-card-6	f	f	2026-06-07 07:20:43.783229
204	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	\N	f	f	2026-06-07 07:20:43.812288
205	\N	8	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Details/1#review-card-5	f	f	2026-06-07 07:21:24.581283
206	\N	5	Report Resolution Update	Your report about the course review has been accepted.	\N	f	f	2026-06-07 07:21:24.598243
207	\N	8	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Details/1#review-card-6	f	f	2026-06-07 07:21:27.80821
208	\N	5	Report Resolution Update	Your report about the course review has been accepted.	\N	f	f	2026-06-07 07:21:27.81965
209	\N	8	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Details/1#review-card-7	f	f	2026-06-07 07:21:32.237247
210	\N	5	Report Resolution Update	Your report about the course review has been accepted.	\N	f	f	2026-06-07 07:21:32.250089
211	\N	1	Course Approved	Your course 'khoa hoc 2' has been approved and is now published in the store. 	/Course/Details/2	f	f	2026-06-07 07:28:08.476573
212	\N	1	New Review	Your course 'khoa hoc 2' just received a 5-star review from a student.	/Course/Learn/2#review-card-7	f	f	2026-06-07 07:28:29.143378
213	\N	1	New Review	Your course 'khoa hoc 2' just received a 5-star review from a student.	/Course/Details/2#review-card-8	f	f	2026-06-07 07:28:33.784601
214	\N	5	Report Resolution Update	Your report about the course review has been reviewed and dismissed.	/Course/Details/2#review-card-8	f	f	2026-06-07 07:29:25.606321
215	\N	5	Report Resolution Update	Your report about the lesson review is currently under review.	/Course/Learn?id=2&lessonId=2#review-card-7	f	f	2026-06-07 07:29:35.918914
216	\N	5	Report Resolution Update	Your report has been escalated to senior administrators.	/Course/Details/2	f	f	2026-06-07 07:30:24.68812
217	\N	2	Report Escalated	A course report has been escalated to you. Report ID: 14	/AdminModeration/Reports	f	f	2026-06-07 07:30:24.699812
218	\N	1	Course Violation Warning	Your course 'khoa hoc 2' has received a policy violation warning. This is Strike 1. Reason: . Please review and correct the content.	/InstructorCourse/Editor/2	f	f	2026-06-07 07:43:44.349665
219	\N	5	Report Resolution Update	Your report has been accepted and appropriate action has been taken.	/Course/Details/2	f	f	2026-06-07 07:43:44.451415
221	\N	5	Report Resolution Update	Your report has been accepted and appropriate action has been taken.	/Course/Details/2	f	f	2026-06-07 07:43:49.563222
222	\N	4	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-07 07:43:52.913767
223	\N	5	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-07 07:43:52.945808
224	\N	7	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-07 07:43:52.94914
225	\N	8	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-07 07:43:52.951675
220	\N	1	Course Violation Warning	Your course 'khoa hoc 2' has received a policy violation warning. This is Strike 2. Reason: . Please review and correct the content.	/InstructorCourse/Editor/2	t	f	2026-06-07 07:43:49.555008
226	\N	9	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-07 07:43:52.955709
228	\N	5	Report Resolution Update	Your report has been accepted and appropriate action has been taken.	/Course/Details/2	f	f	2026-06-07 07:43:52.96591
227	\N	1	Permanent Course Discontinuation Notice	Your course 'khoa hoc 2' has violated our policies. It has been permanently discontinued and archived (Strike 3). New enrollments are disabled. Furthermore, your instructor rights are locked for 30 days (you cannot create, update, or delete courses, lessons, and materials).	/Course/Details/2	t	f	2026-06-07 07:43:52.960377
229	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 2.	/InstructorCourse/Editor/1	f	f	2026-06-07 07:45:34.096829
230	\N	1	Course Unflagged	Your course 'khoa hoc 2' has had a violation flag removed. Current flags: 2.	/InstructorCourse/Editor/2	f	f	2026-06-07 07:57:31.465858
231	\N	1	Course Approved	Your course 'khoa hoc 2' has been approved and is now published in the store. 	/Course/Details/2	f	f	2026-06-07 07:57:37.238225
232	\N	5	Course Content Updated	The course 'khoa hoc 2' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=2	f	f	2026-06-07 07:57:37.269124
233	\N	9	Course Content Updated	The course 'khoa hoc 2' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=2	f	f	2026-06-07 07:57:37.269124
234	\N	1	Course Rejected	Your course 'khoa hoc 2' was not approved. Please check details in the course editor.	/InstructorCourse/Editor?id=2	f	f	2026-06-07 07:57:45.261566
235	\N	1	Permanent Course Discontinuation Notice (3rd Time)	Your course 'khoa hoc 1' has violated policies for the 3rd time. The platform has decided to permanently discontinue this course. You will not be able to edit the content or accept new students, but existing students can still access their purchased content.	/InstructorCourse/Editor?id=1	f	f	2026-06-07 07:58:22.407212
236	\N	2	Manual Audit Required	Course 2 requires manual review following AI Moderation. Threat Level: FlaggedOrRejected.\n\nDetails:\nAI flagged the following issues:\n- [material_3] Found a semantic duplication for material_3 on material_1 (cosine similarity: 1.0 > 0.8)	/AdminModeration/Courses	f	f	2026-06-07 07:59:35.397595
237	\N	1	Course Rejected	Your course 'khoa hoc 2' was not approved. Please check details in the course editor.	/InstructorCourse/Editor?id=2	f	f	2026-06-07 08:29:27.600531
238	\N	4	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-07 08:30:17.844479
239	\N	5	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-07 08:30:17.8942
240	\N	7	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-07 08:30:17.898901
241	\N	8	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-07 08:30:17.901618
242	\N	9	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-07 08:30:17.90657
243	\N	1	Permanent Course Discontinuation Notice	Your course 'khoa hoc 2' has violated our policies. It has been permanently discontinued and archived (Strike 3). New enrollments are disabled. Furthermore, your instructor rights are locked for 30 days (you cannot create, update, or delete courses, lessons, and materials).	/Course/Details/2	f	f	2026-06-07 08:30:17.911573
244	\N	9	Report Resolution Update	Your report has been accepted and appropriate action has been taken.	/Course/Details/2	f	f	2026-06-07 08:30:17.919634
245	\N	9	Community Standards Violation (1st Warning)	Your comment has been removed for violating community standards. This is your first warning.	/Course/Details/2#review-card-8	f	f	2026-06-07 08:45:57.359985
246	\N	5	Report Resolution Update	Your report about the course review has been accepted.	/Course/Details/2#review-card-8	f	f	2026-06-07 08:45:57.468798
247	\N	9	Commenting Restricted (2nd Violation)	Due to repeated violations, you are restricted from posting comments or reviews for 7 days.	/Course/Details/2#review-card-8	f	f	2026-06-07 08:46:05.459815
248	\N	5	Report Resolution Update	Your report about the course review has been accepted.	/Course/Details/2#review-card-8	f	f	2026-06-07 08:46:05.487172
249	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Details/2#review-card-8	f	f	2026-06-07 08:46:18.798362
250	\N	5	Report Resolution Update	Your report about the course review has been accepted.	/Course/Details/2#review-card-8	f	f	2026-06-07 08:46:18.82758
251	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Details/2#review-card-8	f	f	2026-06-07 08:46:59.516296
252	\N	5	Report Resolution Update	Your report about the course review has been accepted.	/Course/Details/2#review-card-8	f	f	2026-06-07 08:46:59.534033
253	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Details/2#review-card-8	f	f	2026-06-07 08:48:03.622347
254	\N	5	Report Resolution Update	Your report about the course review has been accepted.	/Course/Details/2#review-card-8	f	f	2026-06-07 08:48:03.634821
255	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Details/2#review-card-8	f	f	2026-06-07 08:57:58.403595
256	\N	5	Report Resolution Update	Your report about the course review has been accepted.	/Course/Details/2#review-card-8	f	f	2026-06-07 08:57:58.500279
257	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Details/2#review-card-8	f	f	2026-06-07 08:58:23.601658
258	\N	5	Report Resolution Update	Your report about the course review has been accepted.	/Course/Details/2#review-card-8	f	f	2026-06-07 08:58:23.615598
259	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Learn?id=2&lessonId=2#review-card-7	f	f	2026-06-07 08:58:38.932585
261	\N	1	New Review	Your course 'khoa hoc 2' just received a 5-star review from a student.	/Course/Learn/2#review-card-8	f	f	2026-06-07 08:59:54.030275
262	\N	1	New Review	Your course 'khoa hoc 2' just received a 4-star review from a student.	/Course/Details/2#review-card-9	f	f	2026-06-07 09:00:00.888344
263	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Details/2#review-card-8	f	f	2026-06-07 09:00:32.853129
264	\N	5	Report Resolution Update	Your report about the course review has been accepted.	/Course/Details/2#review-card-8	f	f	2026-06-07 09:00:32.867626
265	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Details/2#review-card-9	f	f	2026-06-07 09:01:34.052913
266	\N	5	Report Resolution Update	Your report about the course review has been accepted.	/Course/Details/2#review-card-9	f	f	2026-06-07 09:01:34.068964
267	\N	1	New Review	Your course 'khoa hoc 2' just received a 5-star review from a student.	/Course/Learn/2#review-card-9	f	f	2026-06-07 09:11:01.220395
268	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Learn?id=2&lessonId=2#review-card-8	f	f	2026-06-07 09:12:28.910011
269	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=2&lessonId=2#review-card-8	f	f	2026-06-07 09:12:28.973479
270	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Learn?id=2&lessonId=2#review-card-9	f	f	2026-06-07 09:17:48.317055
271	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=2&lessonId=2#review-card-9	f	f	2026-06-07 09:17:48.584568
272	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Learn?id=2&lessonId=2#review-card-9	f	f	2026-06-07 09:18:18.233484
273	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=2&lessonId=2#review-card-9	f	f	2026-06-07 09:18:18.522758
274	\N	1	New Review	Your course 'khoa hoc 2' just received a 4-star review from a student.	/Course/Learn/2#review-card-10	f	f	2026-06-07 09:27:29.217137
275	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Learn?id=2&lessonId=2#review-card-9	f	f	2026-06-07 09:27:47.318022
276	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=2&lessonId=2#review-card-9	f	f	2026-06-07 09:27:47.413278
277	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Learn?id=2&lessonId=2#review-card-9	f	f	2026-06-07 09:28:37.062661
278	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=2&lessonId=2#review-card-9	f	f	2026-06-07 09:28:37.077862
279	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Learn?id=2&lessonId=2#review-card-9	f	f	2026-06-07 09:32:17.473368
280	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=2&lessonId=2#review-card-9	f	f	2026-06-07 09:32:17.613206
281	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Learn?id=2&lessonId=2#review-card-9	f	f	2026-06-07 09:36:44.963043
282	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=2&lessonId=2#review-card-9	f	f	2026-06-07 09:36:45.125925
283	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Learn?id=2&lessonId=2#review-card-9	f	f	2026-06-07 09:40:48.289606
284	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=2&lessonId=2#review-card-9	f	f	2026-06-07 09:40:48.430826
285	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Learn?id=2&lessonId=2#review-card-10	f	f	2026-06-07 09:42:22.975515
286	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=2&lessonId=2#review-card-10	f	f	2026-06-07 09:42:23.118783
287	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Learn?id=2&lessonId=2#review-card-10	f	f	2026-06-07 09:45:02.567687
288	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=2&lessonId=2#review-card-10	f	f	2026-06-07 09:45:02.583326
289	\N	1	New Review	Your course 'khoa hoc 2' just received a 4-star review from a student.	/Course/Learn/2#review-card-11	f	f	2026-06-07 09:47:52.08159
290	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Learn?id=2&lessonId=2#review-card-11	f	f	2026-06-07 09:48:21.003037
291	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=2&lessonId=2#review-card-11	f	f	2026-06-07 09:48:21.097395
292	\N	1	New Review	Your course 'khoa hoc 2' just received a 3-star review from a student.	/Course/Learn/2#review-card-12	f	f	2026-06-07 09:49:06.160857
293	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Learn?id=2&lessonId=2#review-card-12	f	f	2026-06-07 09:49:24.735791
294	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=2&lessonId=2#review-card-12	f	f	2026-06-07 09:49:24.75312
295	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Learn?id=2&lessonId=2#review-card-12	f	f	2026-06-07 09:51:03.057636
296	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=2&lessonId=2#review-card-12	f	f	2026-06-07 09:51:03.07509
297	\N	9	Review Policy Warning	Your lesson review has received a policy violation warning. Reason: . Please ensure your content complies with our guidelines.	/Course/Learn?id=2&lessonId=2#review-card-12	f	f	2026-06-07 09:51:53.795249
298	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=2&lessonId=2#review-card-12	f	f	2026-06-07 09:51:53.802498
299	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Learn?id=2&lessonId=2#review-card-12	f	f	2026-06-07 09:52:11.824834
300	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=2&lessonId=2#review-card-12	f	f	2026-06-07 09:52:11.838354
301	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Learn?id=2&lessonId=2#review-card-12	f	f	2026-06-07 09:53:44.661214
302	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=2&lessonId=2#review-card-12	f	f	2026-06-07 09:53:44.675118
303	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Learn?id=2&lessonId=2#review-card-12	f	f	2026-06-07 09:54:20.94838
340	\N	5	New Review	Your course 'dasdasd' just received a 5-star review from a student.	/Course/Learn/3#review-card-14	f	f	2026-06-07 10:36:57.336115
304	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=2&lessonId=2#review-card-12	f	f	2026-06-07 09:54:20.962313
305	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Learn?id=2&lessonId=2#review-card-12	f	f	2026-06-07 09:55:01.805034
306	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=2&lessonId=2#review-card-12	f	f	2026-06-07 09:55:01.81689
307	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Learn?id=2&lessonId=2#review-card-12	f	f	2026-06-07 09:55:32.605447
308	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=2&lessonId=2#review-card-12	f	f	2026-06-07 09:55:32.616337
309	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Learn?id=2&lessonId=2#review-card-12	f	f	2026-06-07 10:00:10.442418
310	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=2&lessonId=2#review-card-12	f	f	2026-06-07 10:00:10.545944
311	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Learn?id=2&lessonId=2#review-card-12	f	f	2026-06-07 10:05:59.430262
312	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=2&lessonId=2#review-card-12	f	f	2026-06-07 10:05:59.555466
313	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Learn?id=2&lessonId=2#review-card-12	f	f	2026-06-07 10:06:40.414623
314	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=2&lessonId=2#review-card-12	f	f	2026-06-07 10:06:40.433678
315	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Learn?id=2&lessonId=2#review-card-12	f	f	2026-06-07 10:07:06.794722
316	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=2&lessonId=2#review-card-12	f	f	2026-06-07 10:07:06.811476
317	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Learn?id=2&lessonId=2#review-card-12	f	f	2026-06-07 10:08:18.612823
318	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=2&lessonId=2#review-card-12	f	f	2026-06-07 10:08:18.623563
319	\N	1	New Review	Your course 'khoa hoc 2' just received a 4-star review from a student.	/Course/Learn/2#review-card-13	f	f	2026-06-07 10:20:59.115735
320	\N	1	New Review	Your course 'khoa hoc 2' just received a 5-star review from a student.	/Course/Details/2#review-card-10	f	f	2026-06-07 10:21:04.417094
321	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Learn?id=2&lessonId=2#review-card-12	f	f	2026-06-07 10:21:57.063007
322	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=2&lessonId=2#review-card-12	f	f	2026-06-07 10:21:57.166959
323	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Details/2#review-card-10	f	f	2026-06-07 10:22:22.812951
324	\N	5	Report Resolution Update	Your report about the course review has been accepted.	/Course/Details/2#review-card-10	f	f	2026-06-07 10:22:22.83148
325	\N	9	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Details/2#review-card-10	f	f	2026-06-07 10:22:43.190002
326	\N	5	Report Resolution Update	Your report about the course review has been accepted.	/Course/Details/2#review-card-10	f	f	2026-06-07 10:22:43.203956
327	\N	1	New Review	Your course 'khoa hoc 2' just received a 5-star review from a student.	/Course/Details/2#review-card-11	f	f	2026-06-07 10:24:42.491832
328	\N	1	Course Unflagged	Your course 'khoa hoc 1' has had a violation flag removed. Current flags: 2.	/InstructorCourse/Editor/1	f	f	2026-06-07 10:32:01.687884
329	\N	1	Course Unflagged	Your course 'khoa hoc 2' has had a violation flag removed. Current flags: 2.	/InstructorCourse/Editor/2	f	f	2026-06-07 10:32:27.884155
330	\N	4	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-07 10:32:58.879948
331	\N	5	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-07 10:32:58.938733
332	\N	7	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-07 10:32:58.944747
333	\N	8	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-07 10:32:58.948108
334	\N	9	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-07 10:32:58.952057
335	\N	1	Permanent Course Discontinuation Notice	Your course 'khoa hoc 2' has violated our policies. It has been permanently discontinued and archived (Strike 3). New enrollments are disabled. Furthermore, your instructor rights are locked for 30 days (you cannot create, update, or delete courses, lessons, and materials).	/Course/Details/2	f	f	2026-06-07 10:32:58.955457
336	\N	5	Report Resolution Update	Your report has been accepted and appropriate action has been taken.	/Course/Details/2	f	f	2026-06-07 10:32:58.964851
337	\N	5	Application Result	Congratulations! Your instructor application has been approved.	/InstructorCourse/Create	f	f	2026-06-07 10:34:01.521429
338	\N	2	Manual Audit Required	Course 3 requires manual review following AI Moderation. Threat Level: FlaggedOrRejected.\n\nDetails:\nAI flagged the following issues:\n- [course.title, course.description, course.what_you_will_learn, course.requirements, lesson_3.title, material_4.title] Harmful content detected in fields: course.title, course.description, course.what_you_will_learn, course.requirements, lesson_3.title, material_4.title	/AdminModeration/Courses	f	f	2026-06-07 10:36:17.334196
339	\N	5	Course Approved	Your course 'dasdasd' has been approved and is now published in the store. 	/Course/Details/3	f	f	2026-06-07 10:36:38.403713
341	\N	1	Community Standards Violation (1st Warning)	Your comment has been removed for violating community standards. This is your first warning.	/Course/Learn?id=3&lessonId=3#review-card-14	f	f	2026-06-07 10:37:55.422313
342	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=3&lessonId=3#review-card-14	f	f	2026-06-07 10:37:55.4457
343	\N	1	Commenting Restricted (2nd Violation)	Due to repeated violations, you are restricted from posting comments or reviews for 7 days.	/Course/Learn?id=3&lessonId=3#review-card-14	f	f	2026-06-07 10:37:59.927592
346	\N	4	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-07 10:38:09.217517
349	\N	8	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-07 10:38:09.225867
350	\N	9	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-07 10:38:09.229089
352	\N	1	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Learn?id=3&lessonId=3#review-card-14	f	f	2026-06-07 10:38:43.384007
355	\N	7	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-07 10:38:43.400647
358	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=3&lessonId=3#review-card-14	f	f	2026-06-07 10:38:43.416909
344	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=3&lessonId=3#review-card-14	f	f	2026-06-07 10:37:59.939759
345	\N	1	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Learn?id=3&lessonId=3#review-card-14	f	f	2026-06-07 10:38:09.207158
348	\N	7	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-07 10:38:09.223045
351	\N	5	Report Resolution Update	Your report about the lesson review has been accepted.	/Course/Learn?id=3&lessonId=3#review-card-14	f	f	2026-06-07 10:38:09.237571
354	\N	5	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-07 10:38:43.397868
347	\N	5	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-07 10:38:09.220312
353	\N	4	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-07 10:38:43.395202
356	\N	8	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-07 10:38:43.403307
357	\N	9	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-07 10:38:43.406981
359	\N	2	Manual Audit Required	Course 3 requires manual review following AI Moderation. Threat Level: FlaggedOrRejected.\n\nDetails:\nAI flagged the following issues:\n- [course.title, course.description, course.what_you_will_learn, course.requirements, lesson_5.title] Harmful content detected in fields: course.title, course.description, course.what_you_will_learn, course.requirements, lesson_5.title	/AdminModeration/Courses	f	f	2026-06-08 18:51:40.942754
360	\N	5	Course Approved	Your course 'dasdasd' has been approved and is now published in the store. 	/Course/Details/3	f	f	2026-06-08 18:51:51.667195
361	\N	1	Course Content Updated	The course 'dasdasd' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=3	f	f	2026-06-08 18:51:51.692089
362	\N	4	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-09 11:28:29.692879
363	\N	5	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-09 11:28:29.803318
364	\N	7	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-09 11:28:29.817007
365	\N	8	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-09 11:28:29.82049
366	\N	9	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-09 11:28:29.8253
367	\N	1	Permanent Course Discontinuation Notice	Your course 'khoa hoc 1' has violated our policies. It has been permanently discontinued and archived (Strike 3). New enrollments are disabled. Furthermore, your instructor rights are locked for 30 days (you cannot create, update, or delete courses, lessons, and materials).	/Course/Details/1	f	f	2026-06-09 11:28:29.82821
368	\N	5	Report Resolution Update	Your report has been accepted and appropriate action has been taken.	/Course/Details/1	f	f	2026-06-09 11:28:29.847925
369	\N	5	New Review	Your course 'dasdasd' just received a 5-star review from a student.	/Course/Details/3#review-card-12	f	f	2026-06-09 11:30:12.332394
370	\N	1	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Details/3#review-card-12	f	f	2026-06-09 11:31:07.657101
371	\N	4	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-09 11:31:07.678821
372	\N	5	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-09 11:31:07.685885
373	\N	7	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-09 11:31:07.690681
374	\N	8	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-09 11:31:07.695213
375	\N	9	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-09 11:31:07.701152
376	\N	5	Report Resolution Update	Your report about the course review has been accepted.	/Course/Details/3#review-card-12	f	f	2026-06-09 11:31:07.733441
377	\N	1	Course Unflagged	Your course 'khoa hoc 2' has had a violation flag removed. Current flags: 2.	/InstructorCourse/Editor/2	f	f	2026-06-09 11:32:51.934181
378	\N	5	Report Resolution Update	No issues found on course review, your report has been dismissed.	/Course/Details/3#review-card-12	f	f	2026-06-09 16:48:07.376925
379	\N	5	You have a new order	The course 'dasdasd' has been successfully sold. Expected revenue: $7.52 USD.	/Instructor/Payouts	t	f	2026-06-10 17:05:13.381857
381	\N	5	You have a new order	The course 'dasdasd' has been successfully sold. Expected revenue: $7.52 USD.	/Instructor/Payouts	f	f	2026-06-11 19:21:38.144511
384	\N	1	New Refund Request	A student has submitted a refund request for transaction #4. Reason: sdfsdf	/AdminFinance/Refunds	f	f	2026-06-11 19:46:32.021309
382	\N	5	You have a new order	The course 'dasdasd' has been successfully sold. Expected revenue: $7.52 USD.	/Instructor/Payouts	f	f	2026-06-11 19:38:05.96512
383	\N	5	You have a new order	The course 'dasdasd' has been successfully sold. Expected revenue: $7.52 USD.	/Instructor/Payouts	f	f	2026-06-11 19:42:49.71008
385	\N	5	Community Standards Violation (1st Warning)	Your comment has been removed for violating community standards. This is your first warning.	/Course/Details/3#review-card-13	f	f	2026-06-11 22:22:05.786583
386	\N	8	Report Resolution Update	Your report about the course review has been accepted.	/Course/Details/3#review-card-13	f	f	2026-06-11 22:22:05.864844
387	\N	5	Commenting Restricted (2nd Violation)	Due to repeated violations, you are restricted from posting comments or reviews for 7 days.	/Course/Details/3#review-card-13	f	f	2026-06-11 22:22:21.451499
388	\N	8	Report Resolution Update	Your report about the course review has been accepted.	/Course/Details/3#review-card-13	f	f	2026-06-11 22:22:21.483929
390	\N	4	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-11 22:22:38.876525
393	\N	8	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-11 22:22:38.896502
394	\N	9	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-11 22:22:38.901483
400	\N	11	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-11 22:23:22.777581
403	\N	1	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-11 22:25:10.038151
406	\N	11	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-11 22:25:10.047419
389	\N	1	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Details/3#review-card-12	f	f	2026-06-11 22:22:38.826845
392	\N	7	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-11 22:22:38.893111
395	\N	5	Report Resolution Update	Your report about the course review has been accepted.	/Course/Details/3#review-card-12	f	f	2026-06-11 22:22:38.91285
397	\N	1	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-11 22:23:22.766987
399	\N	10	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-11 22:23:22.7744
404	\N	5	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-11 22:25:10.041115
407	\N	8	Report Resolution Update	Your report about the course review has been accepted.	/Course/Details/3#review-card-13	f	f	2026-06-11 22:25:10.05673
391	\N	5	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-11 22:22:38.889768
396	\N	5	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Details/3#review-card-13	f	f	2026-06-11 22:23:22.754283
398	\N	5	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-11 22:23:22.770973
401	\N	8	Report Resolution Update	Your report about the course review has been accepted.	/Course/Details/3#review-card-13	f	f	2026-06-11 22:23:22.788261
402	\N	5	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Details/3#review-card-13	f	f	2026-06-11 22:25:10.027349
405	\N	10	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-11 22:25:10.044624
408	\N	5	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Details/3#review-card-14	f	f	2026-06-11 22:52:42.023455
409	\N	1	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-11 22:52:42.241897
410	\N	5	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-11 22:52:42.258401
411	\N	10	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-11 22:52:42.263367
412	\N	11	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-11 22:52:42.267923
413	\N	8	Report Resolution Update	Your report about the course review has been accepted.	/Course/Details/3#review-card-14	f	f	2026-06-11 22:52:42.297078
414	\N	5	Account Suspended (3rd Violation)	Your account has been suspended for 30 days due to repeated and severe community standards violations.	/Course/Details/3#review-card-14	f	f	2026-06-11 22:53:18.707298
415	\N	1	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-11 22:53:18.720208
416	\N	5	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-11 22:53:18.723255
417	\N	10	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-11 22:53:18.726876
418	\N	11	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-11 22:53:18.730274
419	\N	8	Report Resolution Update	Your report about the course review has been accepted.	/Course/Details/3#review-card-14	f	f	2026-06-11 22:53:18.743298
420	\N	5	Community Standards Violation (1st Warning)	Your comment has been removed for violating community standards. This is your first warning.	/Course/Details/3#review-card-14	f	f	2026-06-11 22:55:34.790457
421	\N	8	Report Resolution Update	Your report about the course review has been accepted.	/Course/Details/3#review-card-14	f	f	2026-06-11 22:55:34.807905
422	\N	5	Commenting Restricted (2nd Violation)	Due to repeated violations, you are restricted from posting comments or reviews for 7 days.	/Course/Details/3#review-card-14	f	f	2026-06-11 22:55:44.116344
423	\N	8	Report Resolution Update	Your report about the course review has been accepted.	/Course/Details/3#review-card-14	f	f	2026-06-11 22:55:44.132563
424	\N	8	Report Resolution Update	No issues found on course review, your report has been dismissed.	/Course/Details/3#review-card-14	f	f	2026-06-11 22:56:17.148204
425	\N	5	Course Approved	Your course 'dasdasd' has been approved and is now published in the store. 	/Course/Details/3	f	f	2026-06-11 22:58:24.81098
426	\N	1	Course Content Updated	The course 'dasdasd' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=3	f	f	2026-06-11 22:58:24.827378
427	\N	5	Course Content Updated	The course 'dasdasd' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=3	f	f	2026-06-11 22:58:24.827378
428	\N	10	Course Content Updated	The course 'dasdasd' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=3	f	f	2026-06-11 22:58:24.827378
429	\N	11	Course Content Updated	The course 'dasdasd' you enrolled in has been updated with new content. Check it out now!	/Course/Learn?id=3	f	f	2026-06-11 22:58:24.827378
431	\N	8	Report Resolution Update	Your report has been accepted and appropriate action has been taken.	/Course/Details/3	f	f	2026-06-11 23:02:52.586567
430	\N	5	Course Violation Warning	Your course 'dasdasd' has received a policy violation warning. This is Strike 1. Reason: . Please review and correct the content.	/InstructorCourse/Editor/3	t	f	2026-06-11 23:02:52.571512
432	\N	8	Report Resolution Update	Your report is currently under review.	/Course/Details/3	f	f	2026-06-11 23:04:24.175371
433	\N	8	Report Resolution Update	Your report is currently under review.	/Course/Details/3	f	f	2026-06-11 23:04:33.332665
434	\N	8	Report Resolution Update	No issues found on course, your report has been dismissed.	/Course/Details/3	f	f	2026-06-11 23:04:50.733164
435	\N	5	Course Violation Warning	Your course 'dasdasd' has received a policy violation warning. This is Strike 2. Reason: . Please review and correct the content.	/InstructorCourse/Editor/3	f	f	2026-06-11 23:05:16.629117
436	\N	8	Report Resolution Update	Your report has been accepted and appropriate action has been taken.	/Course/Details/3	f	f	2026-06-11 23:05:16.639166
437	\N	1	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-11 23:05:23.372482
441	\N	11	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-11 23:05:23.39117
444	\N	5	Report Resolution Update	No issues found on lesson review, your report has been dismissed.	/Course/Learn?id=2&lessonId=2#review-card-7	f	f	2026-06-11 23:07:12.640621
438	\N	5	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-11 23:05:23.381085
439	\N	8	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-11 23:05:23.38446
442	\N	5	Permanent Course Discontinuation Notice	Your course 'dasdasd' has violated our policies. It has been permanently discontinued and archived (Strike 3). New enrollments are disabled. Furthermore, your instructor rights are locked for 30 days (you cannot create, update, or delete courses, lessons, and materials).	/Course/Details/3	f	f	2026-06-11 23:05:23.394892
443	\N	8	Report Resolution Update	Your report has been accepted and appropriate action has been taken.	/Course/Details/3	f	f	2026-06-11 23:05:23.401485
440	\N	10	Instructor Temporarily Suspended	This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.	\N	f	f	2026-06-11 23:05:23.387478
445	\N	5	Report Resolution Update	Your report about the lesson review is currently under review.	/Course/Learn?id=1&lessonId=1#review-card-4	f	f	2026-06-11 23:07:20.169846
\.


--
-- Data for Name: order_info; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.order_info (order_id, user_id, order_date, order_status, payment_method) FROM stdin;
1	11	2026-06-10 10:05:13.166847	paid	stripe
2	8	2026-06-11 12:21:33.829739	paid	stripe
3	8	2026-06-11 12:38:02.454841	paid	stripe
4	8	2026-06-11 12:42:42.98144	paid	stripe
\.


--
-- Data for Name: order_items; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.order_items (id, order_id, course_id, purchase_price, coupon_used, original_price, coupon_code, coupon_type, discount_amount) FROM stdin;
1	1	3	9.99	f	9.99	\N	\N	0.00
2	2	3	9.99	f	9.99	\N	\N	0.00
3	3	3	9.99	f	9.99	\N	\N	0.00
4	4	3	9.99	f	9.99	\N	\N	0.00
\.


--
-- Data for Name: platform_withdrawals; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.platform_withdrawals (withdrawal_id, manager_id, amount, currency, stripe_payout_id, status, description, created_at, arrived_at) FROM stdin;
\.


--
-- Data for Name: system_configs; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.system_configs (config_id, manager_id, config_key, config_value, description, updated_at) FROM stdin;
1	\N	TransferRate	80	Phần trăm (%) giảng viên nhận được từ mỗi giao dịch. VD: 80 = GV nhận 80%, Sàn giữ 20%.	2026-06-06 00:58:43.453538
2	\N	PayoutDay	15	Ngày trong tháng thực hiện chia tiền cho giảng viên. VD: 15 = ngày 15 hàng tháng.	2026-06-06 00:58:43.454958
3	\N	StripeCountries	[\r\n    {"code":"US","name":"United States"},{"code":"GB","name":"United Kingdom"}\r\n]	Danh sách quốc gia mà Stripe Connect hỗ trợ đăng ký tài khoản Express. Giảng viên chọn 1 trong số này khi đăng ký Stripe.	2026-06-06 00:58:43.455887
4	\N	course_harmful_text_classifier	/app/models/spam_1/,/app/models/toxic_3/	system config of course_harmful_text_classifier	2026-06-11 15:18:30.27369
5	\N	course_text_embedding_generator	distilbert-base-multilingual-cased	system config of course_text_embedding_generator	2026-06-11 15:18:30.332862
6	\N	course_media_embedding_generator	openai/clip-vit-base-patch32	system config of course_media_embedding_generator	2026-06-11 15:18:30.392337
7	\N	review_harmful_text_classifier	/app/models/spam_1/,/app/models/toxic_3/	system config of review_harmful_text_classifier	2026-06-11 15:18:30.45178
8	\N	moderation_threshold	{"similarity":0.85,"spam":0.85,"toxic":0.85}	system config of AI moderation threshold	2026-06-11 15:19:07.39082
\.


--
-- Data for Name: text_embeddings; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.text_embeddings (text_embedding_id, material_id, text_embedding, created_at) FROM stdin;
1	2	[0.019492427,-0.4811205,-0.031576447,0.24091125,0.19258653,0.31867638,0.27537256,0.032831755,-0.10879107,-0.32586944,0.14191832,0.047815673,0.5095626,0.014415904,-0.5717278,-0.21257125,-0.00079461304,0.41897044,-0.030496899,0.24049872,-0.098087706,0.20046586,0.049853854,-0.15741745,0.46409094,-0.3229636,-0.49761006,0.32750574,0.54298264,0.15782176,0.05977668,0.30994746,-0.08100301,0.34673524,-0.008500109,-0.03554678,-1.1807419,0.2174538,-0.10282943,-0.20576905,0.19525784,-0.2127376,0.052642558,-0.04587247,0.0043953666,0.22021674,0.16413234,0.048303746,0.15470907,0.09792927,-0.0351955,-0.48838606,0.27185977,-0.6522508,-0.20072816,0.17888866,0.08140416,-0.0095873,0.0073019085,0.50426877,0.11811842,0.4526661,0.22708958,0.034443386,-0.28752136,-0.11777012,0.2535024,-0.0033215957,-0.1152631,0.30443665,-0.19192182,0.31493926,0.29005155,-0.047275487,-0.045730796,0.01725955,0.21633422,0.15277636,-0.51990443,0.5082129,0.26725233,-0.010732155,-0.37427413,0.10697605,-0.34885955,0.09398103,-0.35580927,-0.1486817,-0.04051225,-0.33500144,-0.1267991,0.18496144,-0.07260481,-0.031024704,-0.4212949,-0.24144857,-0.15836376,0.17777371,-0.0815125,0.027845435,-0.07607119,1.0030016,0.40729105,-0.15879683,-1.791283,-0.05282923,-0.20782134,0.05388823,-0.087222874,0.05202482,0.48466873,0.25066715,0.30994076,0.16074811,0.063490205,0.24097484,-0.4232409,-0.668459,-0.03106871,-0.31285593,0.16883643,0.17813244,0.24836604,-0.39190173,-0.13852917,-0.5154193,0.05687813,-0.62055725,0.43038988,0.6254644,0.14120625,1.1059264,-0.16943423,-0.08301926,-0.029408015,0.7262283,0.24747644,0.36980057,0.11460132,0.2516352,0.27675134,0.12428514,-0.062508546,0.004903093,-0.2852806,-0.15871401,-0.24922477,0.3671653,-0.020960119,-0.1434205,0.11183913,-0.26350948,-0.10088896,0.0012194342,-0.08418186,-0.036151018,0.030698959,-0.040979113,-0.06241834,0.20137273,1.1126714,0.10660414,-0.55931103,0.631485,0.47503275,-0.30929065,0.051057603,0.11457749,-0.08648384,0.07257515,-0.33122262,0.22026601,-0.077083,0.30250698,-0.04835406,-0.17295235,-0.45669848,0.08823888,0.05337769,-0.25848046,-0.3511549,0.1306916,0.17733759,0.31109455,0.32614917,-0.14589056,0.22900006,-0.0100512635,-0.4838834,-0.08285425,0.25793853,0.4677579,0.20516315,0.0351485,0.029406926,0.11722306,-0.05213111,0.09924916,0.0347754,0.015799314,0.22515184,0.043049023,-0.1296077,-0.062460028,-0.029642943,0.53739434,0.118442215,-0.13211067,-0.031247143,0.50712836,-0.07769358,-0.10613291,0.46583995,-0.03799459,0.1862156,0.18796489,0.14500915,0.0047248197,0.20635894,-1.1208464,-0.28355545,-0.1218923,-0.17087816,-0.063587055,-0.4204736,0.13693517,0.25175157,0.1906071,0.020757118,0.12344572,0.38203207,-0.10893086,0.26784927,-1.2718142,-0.4416954,-0.14675963,0.106744714,-0.3613154,-0.446098,-0.13856211,0.10471432,0.12504356,0.14274378,0.09120025,0.47997347,-0.14083871,0.24009806,0.292286,0.12977625,-0.36891553,0.08673902,-0.35879886,0.34448883,0.20674643,-0.058816202,0.14272442,-0.054117873,-0.35164157,0.33229655,-0.0770918,-0.10946721,0.062022977,-0.16523808,0.04880705,-0.34422112,-0.014581665,-0.09507505,0.28389642,0.1465276,-0.24547261,-0.17137872,-0.09932629,0.01629897,-0.1725141,-0.03185765,0.24070913,-0.11436273,0.068284385,-0.14514194,0.080405444,0.12904888,-0.27137038,0.20944242,0.2671947,-0.086811714,-0.041080453,0.053741585,-0.0719195,-0.09297412,-0.610445,-0.021505246,0.10855331,0.23386574,-0.35725772,0.15701418,-0.14826342,0.09601227,-0.12619007,0.28764898,-0.34119177,-0.009564622,-0.421017,0.098555155,0.31001014,-0.85199106,0.10347378,-0.00016785106,0.035204437,-0.15034388,-0.0049250587,-0.32026196,-0.17561187,0.102358915,0.10205824,-0.21244384,-0.079488546,-0.038564216,-0.1278559,-0.26491654,0.05938836,-0.16936272,-0.8552588,0.15508592,0.3395567,0.09190054,0.17747048,-0.13704208,0.07324896,0.096237406,0.18714361,-0.0117738955,0.115014344,-0.18370542,-0.13835487,-0.17839335,-0.034800902,-0.26945907,-0.29445967,0.0798041,0.1040849,-0.05323619,-0.6631635,0.22165106,0.056227773,0.19499642,-0.1047259,-0.28688863,-0.3176333,0.18544501,0.20625973,0.09601931,0.04991599,-0.035751626,-0.22397643,-0.054573316,-0.037855607,-0.5158197,0.05301709,0.10098009,0.23752016,-0.10206208,0.45962995,-0.04613788,0.293825,0.5690082,-0.10690712,-0.37381813,-0.27102742,0.0258925,-0.0024960253,0.20708203,0.42153823,-0.13637172,-0.77198744,0.10808363,0.1830873,-0.053923853,0.29147008,0.5737344,0.03035601,0.18824503,-0.18664159,-0.8115791,0.27096978,-0.012317218,-0.665781,0.14154047,0.20609353,-0.05114315,-0.17266372,0.09015046,-0.026353916,-0.0858477,-0.15360588,-0.03309774,-0.36948505,0.02865145,-0.21706353,0.30799627,0.07805777,-0.10250644,0.38714603,0.057700064,0.15710932,-0.23462352,-1.0347923,0.53421134,0.36635518,-0.043042626,-0.02665331,0.22054194,-0.14742048,0.36753958,0.100116394,0.14472525,0.19644092,-0.03963065,-0.17798004,-0.18099016,-0.13342263,0.23312345,0.5320931,0.028210282,-0.1508895,0.082153924,-0.033373952,-0.0004027877,-0.22928303,-0.29680896,0.2345267,-0.6198446,0.056228105,-0.020327076,-0.5438162,-1.1295204,0.4579084,-0.29305905,-0.20822321,0.043254133,1.4040527,0.09158708,0.68620795,-0.04817215,0.039693426,0.02716941,2.3755133,0.5963565,-0.50911635,0.5433666,0.007082739,0.18980956,-0.6077196,-0.8382936,-1.1794251,-0.1290502,-0.03985529,-0.29610637,-0.01101716,-0.26126802,-0.2052571,0.18939036,-0.023041014,-0.1690567,0.043631658,0.22885934,-0.46514484,-0.3753363,-0.020108242,0.22469959,0.19201021,0.17104976,0.48208776,0.0055004265,-0.45769724,-0.15245256,-1.1287138,0.12260975,-0.24435349,0.25970653,0.10697442,-0.08008089,0.29440153,0.22775555,-0.32288423,0.19634946,-0.035681553,-0.22304401,0.18361485,-0.4010045,0.041723475,0.37726754,0.10922593,-0.25547042,0.8918205,-0.20889926,-0.032760676,-0.23965971,0.24057202,-0.34694192,-0.2262691,-0.055061232,-0.081784286,-0.026728814,0.08006014,-0.10403471,-0.011748204,-0.53350616,-0.04892209,-0.13307096,0.022644978,0.27838436,-0.7178663,-0.13755031,-0.038711652,0.06576734,-0.025664775,-0.123115875,-0.1649164,-0.35215512,-0.03551661,0.049816918,0.37803394,0.16768669,-0.21259396,-0.38543206,-0.034136012,-0.47038037,0.04900908,0.03542651,-0.0017450213,0.09937052,-0.2984182,-0.27046132,1.2210112,0.04438002,0.37239546,0.4991761,-0.1439721,0.032656427,0.16998968,-0.05512647,0.037310354,-0.21498755,-0.28020015,0.075002424,0.21782473,-0.051004704,-0.095047,-0.024501434,0.21949542,0.22840255,0.33079028,-0.0662524,0.12507817,-0.18718185,0.15926534,-0.22615086,-0.20537838,0.0023935298,0.16268015,0.21547323,0.22360149,-0.23440458,-0.19869639,-0.045077074,-0.046641618,-0.123747475,0.088640094,-0.0083396,0.04185064,0.14483018,0.23188205,-0.32015705,-0.028788634,-0.2630692,0.094442554,-0.07560794,0.517298,0.39485607,-0.3510801,-0.010450976,-0.48831078,0.17688024,0.21435948,-0.35194156,0.0070475587,0.036057465,0.28629142,-0.19770919,-0.17862089,-0.06074688,0.06591651,-0.2598811,-0.097024016,0.23049511,0.30449793,0.61949015,0.020244317,0.012517627,-0.28575978,0.09616643,-0.40685508,0.3474597,-0.07346894,-0.07904588,-0.14760412,0.21804634,0.19541217,-0.27166742,0.30837455,0.02470884,-0.002931095,0.013993974,-0.15516362,0.10174344,-1.4921768,-0.13843787,0.056650035,0.115313314,0.12977345,0.39074787,0.94741446,-0.013056243,-0.08939974,-0.11878737,-0.21020767,-0.11322514,0.046815056,0.08417203,-1.0741695,0.08676297,-0.08411545,0.14785782,-0.22811632,-0.11886509,-0.7302485,-0.3501294,-0.17606184,0.26734462,-0.54732126,-0.06938579,-0.3314878,-0.084945634,-0.2195151,0.055949133,-0.123522736,-0.19170597,0.13671722,-0.08968904,0.028145688,0.43857437,0.08285497,-0.21551375,0.035305418,0.34548774,-0.04466275,-0.11618975,-0.2267785,-0.2953815,0.38944227,0.42307663,0.4424395,0.32391328,0.0795767,0.4609536,-0.7994804,-0.10999404,-0.09483883,-0.058160637,0.44636095,-0.0860725,-0.51206046,0.108731,0.14427589,0.13904855,-0.28205797,-0.14025697,-0.077461384,0.4771317,0.055142898,0.1878679,0.17349398,0.49837664,-0.35390696,0.059064586,0.19030188,-0.1705747,0.5556112,-0.20535327,0.25341314,0.5871737,0.38983893,0.45953903,-0.05019359,-0.13198155,0.11148335,-0.5341006,-0.034000147,0.1644309,-0.18316695,0.46966398,-0.33762154,0.22529489,0.049658526,-0.019366834,0.32816228,0.4457383,0.73890066,0.007956816,0.13993426,0.30767277,0.011624371,-0.3431689,-0.20803013,-0.091377005,0.8039188,0.17478126,-0.24056537,-0.22911908,0.68697685,-0.03546385,1.0102845,-0.2932917,-0.12670542,-0.31925228,0.040587895,0.2420462,-0.113677606,-0.21068968,-0.295492,-0.001482309,-1.9092499,-0.10491336,0.4257172,0.30977246,-0.1624844,-0.5566278,0.55974615,-0.08505992,0.16844614,-0.021240786,-0.014434892,-0.22674866,0.086855076,0.28093055,0.28698635,-0.1994575,0.25264415,-0.20388168,-0.2611225,0.09986338,-0.26964587,0.59339285,-0.19777526,0.1452016,0.61798835,-0.08968025,-0.22842526,-0.014702576,-0.048890155,-0.68834156,-0.4495597,0.15747996,0.3354038,0.19184351,0.44962013,0.19819055,-0.15091139]	2026-06-06 16:24:03.718151
2	8	[-0.046201054,-0.2145713,0.118783616,0.087805375,0.1444102,0.08261062,0.21996708,-0.22741815,0.058395594,-0.024899673,0.1355956,-0.08562475,-0.08027652,0.2724928,-0.65408593,-0.15860356,-0.22382909,0.08371042,-0.108658426,0.3823214,0.09498728,-0.06555219,-0.0003282928,0.07594582,0.31088564,-0.47639,-0.31971166,0.3195236,0.37461728,0.23093459,0.045479506,0.31605622,0.13010295,0.29860044,-0.15216424,0.024403807,-1.8346306,-0.23385,0.019952653,-0.36639813,-0.04765489,0.08699894,0.21937497,-0.09471522,-0.045226805,1.0580952,0.13432829,-0.029418819,0.89611286,0.067332834,-0.06672205,-0.69236517,0.14364153,-1.364224,-0.08756624,0.14836255,0.09883695,-0.26055387,-0.11077798,0.21984783,0.118297294,0.043318097,0.17862467,0.23271643,-0.41537458,-0.11806903,-0.113355495,-0.012943243,-0.23509772,0.09084047,0.16045067,0.08135439,0.12473728,-0.3020402,-0.08285668,-0.055004463,-0.028369619,0.14985926,0.12084819,-0.05714184,0.015256972,-0.04116382,-0.5464338,0.08328694,-0.26495346,-0.042439315,-0.19004655,-0.07366932,-0.043709856,-0.056064695,-0.016363826,-0.21568592,-0.4897029,-0.02846753,-0.58390754,-0.54322225,0.109180115,-0.13617739,0.03589863,-0.23036274,-0.04277965,1.4699804,0.11223777,0.07587681,-2.6907506,0.10859055,-0.24277622,0.098100804,0.21469466,-0.24609093,0.18672119,-0.058672097,0.26787692,0.061884135,0.16060355,-0.05455219,-0.051816385,-0.17938398,-0.24341196,-0.06685684,-0.119231366,0.084759966,0.13569595,-0.119745195,0.05799963,-0.38197908,-0.088848084,-0.20535605,0.44878626,0.8510532,0.247834,1.6680915,0.07874976,-0.1742193,0.09101741,1.1811837,-0.22747208,0.28747943,0.12587762,0.100095704,0.23323101,-0.0671289,-0.17665982,-0.25798258,-0.029509466,-0.09785109,-0.1156869,0.077210434,-0.10527609,-0.14588279,-0.27203462,-0.32069474,-0.19618097,-0.11011425,0.009681718,-0.07707934,0.028602669,0.026608111,0.082446836,0.48669115,1.7093912,-0.013902649,-0.22435544,0.5020798,0.24797879,-0.016880143,-0.034594048,0.15875958,0.028511042,-0.32751873,-0.36934054,-0.4287684,0.1057296,0.18249013,-0.18273701,-0.22431432,-0.30202508,0.03561795,-0.06604605,0.04695956,-0.056473434,0.18637468,0.11127671,0.08525752,0.36305514,0.22969659,0.28215525,0.2151215,-0.43318722,-0.34081575,0.448658,0.41710016,0.1873941,-0.079324335,0.09410122,0.23952176,0.052016784,0.12119056,0.045603845,-0.041049007,0.48633683,-0.014283808,0.09077098,0.11503386,0.11631661,0.7219546,0.11207387,-0.18171628,0.14122804,0.70020676,-0.09233287,0.015380367,0.86143935,0.20188108,0.1629017,0.14103682,0.028519092,0.13464038,0.41343468,-1.8064104,-0.067064546,0.062855825,-0.024339044,-0.07315466,-0.2931349,-0.08190628,0.26513356,0.45140085,0.041905224,-0.08465518,-0.082812265,-0.17787412,0.23242702,-1.7440927,-0.023148552,-0.10443929,0.2823709,-0.14811876,-0.053218223,-0.19844957,-0.04395863,0.030402346,0.08062523,-0.09702129,0.25284368,-0.04628475,0.63149613,-0.10288299,0.34991044,-0.10474786,-0.20077474,0.2740374,0.3572436,0.17224507,-0.29527962,0.11610885,0.18578662,-0.2868941,0.49164367,-0.0007600384,-0.18585616,-0.24110338,-0.3113368,-0.23730628,-0.15757436,0.037913144,0.25092798,0.24185401,0.11657213,-0.25251594,-0.16523223,0.14820613,-0.16684064,-0.08957507,-0.098806426,-0.08230888,0.026080856,0.36883548,0.027589457,0.21100175,0.21881169,-0.27313045,-0.011734566,0.22708678,-0.22129731,-0.042900853,0.088944055,-0.23490642,-0.47281379,-1.1141784,-0.07618219,0.014151788,0.08566353,-0.07805322,-0.18201412,-0.31937334,-0.17689586,-0.20561104,0.02889647,-0.05670013,0.09584709,-0.36214262,0.18028244,-0.10848549,-1.3447083,0.26120138,0.13566141,0.07460096,-0.02044199,-0.06357749,0.009177119,0.014837073,0.025795469,-0.23843183,-0.22550665,-0.20028101,0.13381436,0.3166822,-0.168035,0.18265624,-0.27192533,-1.5194956,0.00065232377,0.14598204,-0.137149,0.25595778,0.0748479,0.11089734,-0.17970882,0.2785723,0.03415009,-0.012199023,-0.037601396,-0.2785139,-0.097355165,0.14812183,-0.24954188,0.006793711,0.105802044,-0.22210172,-0.24687567,-0.07704293,0.105636306,-0.24351652,0.087794065,0.104487464,-0.27810034,-0.16054723,0.24920888,0.37199596,0.08985807,0.21999764,-0.3920102,0.065730646,-0.028688936,0.48278072,-0.12554587,0.045678705,-0.024242923,0.1459859,-0.17208499,0.14896914,0.051210746,0.21125542,0.12027177,0.033381514,-0.1626877,-0.43979722,0.0437341,-0.18813275,0.17857562,0.42482275,-0.2163893,-0.22338854,0.14702758,0.049088646,-0.025213493,0.11656895,0.08208127,0.043838568,-0.0015380081,-0.16911493,-1.0844746,0.14983849,0.12965022,-1.6045223,0.41497263,0.04274731,-0.21399982,-0.2427427,-0.030675564,0.089907415,0.22426616,-0.13967046,-0.097193986,-0.027795874,-0.09047043,-0.23712417,0.12030927,0.031101089,0.10810423,-0.21571293,0.14172333,-0.069350556,-0.28013366,-1.449987,0.1524462,0.1794184,-0.072162226,-0.07040178,-0.049380526,-0.10401656,0.24760714,-0.29046673,-0.0054987567,0.07035557,0.026899805,-0.26039973,-0.09892186,-0.39682224,0.064648084,0.4842219,-0.023390623,-0.5599493,-0.011279352,-0.05361123,-0.006774656,0.09296054,-0.09967726,0.17972817,-0.4093415,0.16619655,0.24535209,-1.0584925,-1.9588411,0.95803016,-0.07713526,0.10700636,0.042745784,2.1824245,0.04452487,0.41127974,-0.06878879,0.029444942,-0.15185446,3.094279,0.47029567,-0.35981157,1.3590487,-0.12529762,0.36446702,-0.30949092,-1.5202177,-1.9318157,-0.1323622,-0.4525413,-0.06496045,-0.1280989,0.13404201,-0.11898884,0.3922933,0.089926526,-0.06427752,0.12819174,-0.01590828,0.036796167,-0.42278108,-0.0038802007,0.034246035,0.19700101,0.83539134,0.098191746,-0.081859596,0.24798654,-0.174063,-1.7372603,0.13812119,-0.090487644,0.13239552,-0.059725065,-0.037843525,0.13450722,-0.18971811,-0.1423115,0.06537816,0.0025465463,0.25613117,0.10864788,0.026334254,-0.011734142,0.30411786,0.018158553,-0.09427736,1.8746969,0.15929732,-0.15258737,0.36305016,0.2566171,-0.25410968,-0.237311,0.04226865,-0.37345058,-0.042813938,0.1706014,-0.14362866,-0.2015262,-0.49005282,0.23044348,-0.22407816,-0.005520226,0.48198202,0.1410461,0.0384216,-0.6954038,0.041054044,-0.14466004,0.02639364,-0.122809194,-0.25686508,-0.12425987,0.07926323,-0.054902136,0.113564014,-0.09809085,-0.059456147,-0.33627933,-0.3979663,0.03887309,-0.12102402,0.06096701,-0.19018611,-0.59199107,0.012668616,1.3315984,0.01747928,-0.105547436,0.8357236,0.091732025,0.054001976,0.34989706,0.047968823,-0.084379375,0.044442203,-0.17490315,-0.06185544,-0.091260865,0.36953685,-0.20518531,0.19548121,0.098024495,0.48715013,0.52210355,0.16321455,0.2519524,-0.3312927,-0.17285992,0.08058374,-0.30396563,-0.046253804,-0.13480964,0.09575711,0.18928778,0.06496703,0.005611297,-0.18792148,-0.020974554,-0.11082207,0.33308902,0.07001521,-0.19145769,0.0066117947,0.056014042,0.07037036,0.13396229,-0.25498778,-0.12604263,-0.15710974,0.37902272,0.25315097,-0.18126126,-0.14164194,-0.49787903,-0.04988157,-0.13205326,-0.5686704,0.1190115,0.0612407,-0.19026403,-0.1547935,-0.15511373,-0.07363194,0.1485203,0.0006240397,-0.06767416,0.20372467,0.14525953,0.20683132,0.06337033,-0.047470745,-0.0762719,0.30487502,0.0070472048,0.27784157,0.040027265,-0.2304011,-0.12622264,0.19229636,0.14566076,-0.3093642,0.030466143,0.011315708,0.11848328,0.027564486,0.21878375,0.2592045,-2.0224597,0.029005114,0.078105316,0.21087378,-0.16914624,0.14148436,0.9008817,-0.0619092,-0.25680932,0.014235347,-0.006312734,0.12750486,-0.09191028,-0.17721507,-1.1248498,-0.12492515,-0.07098495,0.21782742,-0.22555976,0.05991067,-1.6206697,-0.2676517,-0.4512526,0.090218164,-0.23218484,0.15328084,-0.08440202,-0.27358776,-0.040339448,-0.15177524,0.029013202,-0.14374925,0.23796435,-0.07434371,-0.02382824,0.13505606,0.24166419,0.23984662,-0.121859826,0.0656893,-0.14672348,0.13119487,-0.471187,0.07840701,0.65634954,0.3614589,0.13900596,0.342794,-0.34242073,0.21233575,-1.6648144,-0.15299232,-0.2594729,-0.06889673,0.2064759,0.35603538,0.13065465,0.04610686,-0.20086917,0.15940252,-0.47413075,-0.13794827,-0.029635925,0.24788089,0.12952282,-0.2541196,0.4524709,0.3026219,-0.37300143,0.1803725,0.034445733,-0.20453869,0.46101722,0.025641186,0.38025028,0.62546796,0.3623754,0.2553874,-0.29458824,-0.07322046,-0.23324506,-0.94586825,0.0671238,0.2141734,-0.31930977,0.14553894,-0.1597857,0.083651364,0.14140147,0.3889309,0.07173454,0.4129111,1.7751254,-0.050066028,0.017788142,0.32088357,0.09273063,-0.12801273,-0.1438142,-0.043557473,1.6239868,-0.030903853,0.030704075,-0.029900268,0.57642794,-0.02774552,1.5262408,-0.13892724,0.12044847,0.22108586,0.04875137,0.19536725,0.30532184,-0.01586512,-0.14774895,-0.14049698,-1.0688128,-0.11916595,0.295416,0.3408552,-0.18837753,-0.64812636,0.65992343,-0.09262037,0.19311662,-0.17004992,0.013102876,-0.26107627,0.16156112,0.22477485,0.059524085,-0.16391203,0.20304808,0.11239534,-0.100403026,0.18621585,-0.05507014,0.44301245,-0.35807347,-0.0050720833,0.70074666,-0.09285134,-0.18486556,0.08573204,-0.41610345,-0.41918287,-0.40205526,0.043022208,0.40947106,0.027168322,0.4993713,0.15979384,0.05993232]	2026-06-08 11:51:40.902915
\.


--
-- Data for Name: transaction_exts; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.transaction_exts (transaction_id, refund_reason, refund_admin_note, refund_requested_at) FROM stdin;
4	sdfsdf	This gift has already been claimed, refund is not allowed.	2026-06-11 12:46:32.016312
\.


--
-- Data for Name: transactions; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.transactions (transaction_id, order_item_id, account_from, account_to, amount, transfer_rate, stripe_session_id, stripe_paymentintent_id, currency, transactions_status, transaction_type, transaction_created_at) FROM stdin;
1	1	11	5	9.99	80.00	pi_3Tgj0R2VKa98yCkd04yQqBIH	pi_3Tgj0R2VKa98yCkd04yQqBIH	usd	succeeded	payment	2026-06-10 10:05:13.262605
2	2	8	5	9.99	80.00	pi_3Th7bb2VKa98yCkd0iq33ivA	pi_3Th7bb2VKa98yCkd0iq33ivA	usd	succeeded	payment	2026-06-11 12:21:33.973999
3	3	8	5	9.99	80.00	pi_3Th7rx2VKa98yCkd1Zb8yZDd	pi_3Th7rx2VKa98yCkd1Zb8yZDd	usd	succeeded	payment	2026-06-11 12:38:02.474189
4	4	8	5	9.99	80.00	pi_3Th7wa2VKa98yCkd0vrTx0H3	pi_3Th7wa2VKa98yCkd0vrTx0H3	usd	succeeded	payment	2026-06-11 12:42:42.994078
\.


--
-- Data for Name: user_avatar_frames; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.user_avatar_frames (user_id, frame_id, unlocked_at, is_equipped) FROM stdin;
\.


--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.users (user_id, full_name, bio, date_of_birth) FROM stdin;
1	instructor	\N	\N
4	kien an	\N	\N
7	Kien An Huynh (CE191266)	\N	\N
8	An Kien	\N	\N
9	Kien An Huynh	\N	\N
10	huynh kien an	\N	\N
11	huynh kien an	\N	\N
5	Kien Ant	\N	2015-01-10
\.


--
-- Data for Name: wishlist_items; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.wishlist_items (id, user_id, course_id, added_date) FROM stdin;
\.


--
-- Name: accounts_account_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.accounts_account_id_seq', 14, true);


--
-- Name: ai_models_model_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ai_models_model_id_seq', 12, true);


--
-- Name: audit_logs_log_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.audit_logs_log_id_seq', 1, false);


--
-- Name: avatar_frames_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.avatar_frames_id_seq', 3, true);


--
-- Name: cart_items_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.cart_items_id_seq', 2, true);


--
-- Name: categories_category_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.categories_category_id_seq', 13, true);


--
-- Name: chats_chat_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.chats_chat_id_seq', 4, true);


--
-- Name: coupons_coupon_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.coupons_coupon_id_seq', 1, false);


--
-- Name: course_ai_usage_logs_log_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.course_ai_usage_logs_log_id_seq', 30, true);


--
-- Name: course_reports_course_report_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.course_reports_course_report_id_seq', 25, true);


--
-- Name: course_review_moderation_logs_log_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.course_review_moderation_logs_log_id_seq', 12, true);


--
-- Name: course_review_reports_course_review_report_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.course_review_reports_course_review_report_id_seq', 40, true);


--
-- Name: course_reviews_course_review_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.course_reviews_course_review_id_seq', 14, true);


--
-- Name: courses_ai_integrations_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.courses_ai_integrations_id_seq', 9, true);


--
-- Name: courses_course_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.courses_course_id_seq', 3, true);


--
-- Name: enrollments_enrollment_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.enrollments_enrollment_id_seq', 14, true);


--
-- Name: gifts_gift_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.gifts_gift_id_seq', 3, true);


--
-- Name: instructor_payouts_payout_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.instructor_payouts_payout_id_seq', 4, true);


--
-- Name: learning_materials_material_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.learning_materials_material_id_seq', 8, true);


--
-- Name: lesson_review_moderation_logs_log_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.lesson_review_moderation_logs_log_id_seq', 12, true);


--
-- Name: lesson_review_reports_lesson_review_report_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.lesson_review_reports_lesson_review_report_id_seq', 41, true);


--
-- Name: lesson_reviews_lesson_review_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.lesson_reviews_lesson_review_id_seq', 14, true);


--
-- Name: lessons_lesson_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.lessons_lesson_id_seq', 5, true);


--
-- Name: lockouts_lockout_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.lockouts_lockout_id_seq', 65, true);


--
-- Name: material_completions_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.material_completions_id_seq', 10, true);


--
-- Name: media_embeddings_media_embedding_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.media_embeddings_media_embedding_id_seq', 3, true);


--
-- Name: message_attachments_attachment_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.message_attachments_attachment_id_seq', 1, false);


--
-- Name: message_moderation_logs_log_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.message_moderation_logs_log_id_seq', 1, false);


--
-- Name: messages_message_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.messages_message_id_seq', 4, true);


--
-- Name: notifications_notification_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.notifications_notification_id_seq', 445, true);


--
-- Name: order_info_order_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.order_info_order_id_seq', 4, true);


--
-- Name: order_items_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.order_items_id_seq', 4, true);


--
-- Name: platform_withdrawals_withdrawal_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.platform_withdrawals_withdrawal_id_seq', 1, false);


--
-- Name: system_configs_config_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.system_configs_config_id_seq', 8, true);


--
-- Name: text_embeddings_text_embedding_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.text_embeddings_text_embedding_id_seq', 2, true);


--
-- Name: transactions_transaction_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.transactions_transaction_id_seq', 4, true);


--
-- Name: wishlist_items_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.wishlist_items_id_seq', 1, false);


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- Name: accounts accounts_email_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.accounts
    ADD CONSTRAINT accounts_email_key UNIQUE (email);


--
-- Name: accounts accounts_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.accounts
    ADD CONSTRAINT accounts_pkey PRIMARY KEY (account_id);


--
-- Name: accounts accounts_username_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.accounts
    ADD CONSTRAINT accounts_username_key UNIQUE (username);


--
-- Name: ai_models ai_models_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ai_models
    ADD CONSTRAINT ai_models_pkey PRIMARY KEY (model_id);


--
-- Name: audit_logs audit_logs_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.audit_logs
    ADD CONSTRAINT audit_logs_pkey PRIMARY KEY (log_id);


--
-- Name: avatar_frames avatar_frames_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.avatar_frames
    ADD CONSTRAINT avatar_frames_pkey PRIMARY KEY (id);


--
-- Name: cart_items cart_items_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cart_items
    ADD CONSTRAINT cart_items_pkey PRIMARY KEY (id);


--
-- Name: cart_items cart_items_user_id_course_id_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cart_items
    ADD CONSTRAINT cart_items_user_id_course_id_key UNIQUE (user_id, course_id);


--
-- Name: categories categories_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.categories
    ADD CONSTRAINT categories_pkey PRIMARY KEY (category_id);


--
-- Name: chat_participants chat_participants_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.chat_participants
    ADD CONSTRAINT chat_participants_pkey PRIMARY KEY (chat_id, account_id);


--
-- Name: chats chats_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.chats
    ADD CONSTRAINT chats_pkey PRIMARY KEY (chat_id);


--
-- Name: coupons coupons_coupon_code_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.coupons
    ADD CONSTRAINT coupons_coupon_code_key UNIQUE (coupon_code);


--
-- Name: coupons coupons_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.coupons
    ADD CONSTRAINT coupons_pkey PRIMARY KEY (coupon_id);


--
-- Name: course_ai_usage_logs course_ai_usage_logs_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.course_ai_usage_logs
    ADD CONSTRAINT course_ai_usage_logs_pkey PRIMARY KEY (log_id);


--
-- Name: course_exts course_exts_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.course_exts
    ADD CONSTRAINT course_exts_pkey PRIMARY KEY (course_id);


--
-- Name: course_reports course_reports_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.course_reports
    ADD CONSTRAINT course_reports_pkey PRIMARY KEY (course_report_id);


--
-- Name: course_review_moderation_logs course_review_moderation_logs_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.course_review_moderation_logs
    ADD CONSTRAINT course_review_moderation_logs_pkey PRIMARY KEY (log_id);


--
-- Name: course_review_reports course_review_reports_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.course_review_reports
    ADD CONSTRAINT course_review_reports_pkey PRIMARY KEY (course_review_report_id);


--
-- Name: course_reviews course_reviews_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.course_reviews
    ADD CONSTRAINT course_reviews_pkey PRIMARY KEY (course_review_id);


--
-- Name: courses_ai_integrations courses_ai_integrations_model_id_course_id_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.courses_ai_integrations
    ADD CONSTRAINT courses_ai_integrations_model_id_course_id_key UNIQUE (model_id, course_id);


--
-- Name: courses_ai_integrations courses_ai_integrations_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.courses_ai_integrations
    ADD CONSTRAINT courses_ai_integrations_pkey PRIMARY KEY (id);


--
-- Name: courses courses_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.courses
    ADD CONSTRAINT courses_pkey PRIMARY KEY (course_id);


--
-- Name: enrollments enrollments_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.enrollments
    ADD CONSTRAINT enrollments_pkey PRIMARY KEY (enrollment_id);


--
-- Name: enrollments enrollments_user_id_course_id_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.enrollments
    ADD CONSTRAINT enrollments_user_id_course_id_key UNIQUE (user_id, course_id);


--
-- Name: gifts gifts_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.gifts
    ADD CONSTRAINT gifts_pkey PRIMARY KEY (gift_id);


--
-- Name: gifts gifts_redemption_token_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.gifts
    ADD CONSTRAINT gifts_redemption_token_key UNIQUE (redemption_token);


--
-- Name: instructor_payouts instructor_payouts_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.instructor_payouts
    ADD CONSTRAINT instructor_payouts_pkey PRIMARY KEY (payout_id);


--
-- Name: instructors instructors_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.instructors
    ADD CONSTRAINT instructors_pkey PRIMARY KEY (instructor_id);


--
-- Name: learning_materials learning_materials_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.learning_materials
    ADD CONSTRAINT learning_materials_pkey PRIMARY KEY (material_id);


--
-- Name: lesson_review_moderation_logs lesson_review_moderation_logs_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.lesson_review_moderation_logs
    ADD CONSTRAINT lesson_review_moderation_logs_pkey PRIMARY KEY (log_id);


--
-- Name: lesson_review_reports lesson_review_reports_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.lesson_review_reports
    ADD CONSTRAINT lesson_review_reports_pkey PRIMARY KEY (lesson_review_report_id);


--
-- Name: lesson_reviews lesson_reviews_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.lesson_reviews
    ADD CONSTRAINT lesson_reviews_pkey PRIMARY KEY (lesson_review_id);


--
-- Name: lessons lessons_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.lessons
    ADD CONSTRAINT lessons_pkey PRIMARY KEY (lesson_id);


--
-- Name: lockouts lockouts_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.lockouts
    ADD CONSTRAINT lockouts_pkey PRIMARY KEY (lockout_id);


--
-- Name: managers managers_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.managers
    ADD CONSTRAINT managers_pkey PRIMARY KEY (manager_id);


--
-- Name: material_completions material_completions_enrollment_id_material_id_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.material_completions
    ADD CONSTRAINT material_completions_enrollment_id_material_id_key UNIQUE (enrollment_id, material_id);


--
-- Name: material_completions material_completions_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.material_completions
    ADD CONSTRAINT material_completions_pkey PRIMARY KEY (id);


--
-- Name: media_embeddings media_embeddings_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.media_embeddings
    ADD CONSTRAINT media_embeddings_pkey PRIMARY KEY (media_embedding_id);


--
-- Name: message_attachments message_attachments_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.message_attachments
    ADD CONSTRAINT message_attachments_pkey PRIMARY KEY (attachment_id);


--
-- Name: message_moderation_logs message_moderation_logs_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.message_moderation_logs
    ADD CONSTRAINT message_moderation_logs_pkey PRIMARY KEY (log_id);


--
-- Name: messages messages_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.messages
    ADD CONSTRAINT messages_pkey PRIMARY KEY (message_id);


--
-- Name: notifications notifications_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.notifications
    ADD CONSTRAINT notifications_pkey PRIMARY KEY (notification_id);


--
-- Name: order_info order_info_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.order_info
    ADD CONSTRAINT order_info_pkey PRIMARY KEY (order_id);


--
-- Name: order_items order_items_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.order_items
    ADD CONSTRAINT order_items_pkey PRIMARY KEY (id);


--
-- Name: platform_withdrawals platform_withdrawals_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.platform_withdrawals
    ADD CONSTRAINT platform_withdrawals_pkey PRIMARY KEY (withdrawal_id);


--
-- Name: system_configs system_configs_config_key_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.system_configs
    ADD CONSTRAINT system_configs_config_key_key UNIQUE (config_key);


--
-- Name: system_configs system_configs_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.system_configs
    ADD CONSTRAINT system_configs_pkey PRIMARY KEY (config_id);


--
-- Name: text_embeddings text_embeddings_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.text_embeddings
    ADD CONSTRAINT text_embeddings_pkey PRIMARY KEY (text_embedding_id);


--
-- Name: transaction_exts transaction_exts_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.transaction_exts
    ADD CONSTRAINT transaction_exts_pkey PRIMARY KEY (transaction_id);


--
-- Name: transactions transactions_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.transactions
    ADD CONSTRAINT transactions_pkey PRIMARY KEY (transaction_id);


--
-- Name: course_exts uq_description_hash; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.course_exts
    ADD CONSTRAINT uq_description_hash UNIQUE (description_hash);


--
-- Name: course_exts uq_requirements_hash; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.course_exts
    ADD CONSTRAINT uq_requirements_hash UNIQUE (requirements_hash);


--
-- Name: course_exts uq_thumbnail_hash; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.course_exts
    ADD CONSTRAINT uq_thumbnail_hash UNIQUE (thumbnail_hash);


--
-- Name: course_exts uq_title_hash; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.course_exts
    ADD CONSTRAINT uq_title_hash UNIQUE (title_hash);


--
-- Name: course_exts uq_what_you_will_learn_hash; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.course_exts
    ADD CONSTRAINT uq_what_you_will_learn_hash UNIQUE (what_you_will_learn_hash);


--
-- Name: user_avatar_frames user_avatar_frames_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_avatar_frames
    ADD CONSTRAINT user_avatar_frames_pkey PRIMARY KEY (user_id, frame_id);


--
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (user_id);


--
-- Name: wishlist_items wishlist_items_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.wishlist_items
    ADD CONSTRAINT wishlist_items_pkey PRIMARY KEY (id);


--
-- Name: wishlist_items wishlist_items_user_id_course_id_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.wishlist_items
    ADD CONSTRAINT wishlist_items_user_id_course_id_key UNIQUE (user_id, course_id);


--
-- Name: idx_audit_logs_actor; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_audit_logs_actor ON public.audit_logs USING btree (actor_id);


--
-- Name: idx_chat_participants_read; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_chat_participants_read ON public.chat_participants USING btree (account_id, last_read_at);


--
-- Name: idx_course_reviews_active; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_course_reviews_active ON public.course_reviews USING btree (enrollment_id) WHERE (is_removed = false);


--
-- Name: idx_course_reviews_enrollment; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_course_reviews_enrollment ON public.course_reviews USING btree (enrollment_id);


--
-- Name: idx_courses_instructor; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_courses_instructor ON public.courses USING btree (instructor_id);


--
-- Name: idx_enrollments_course; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_enrollments_course ON public.enrollments USING btree (course_id);


--
-- Name: idx_enrollments_user; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_enrollments_user ON public.enrollments USING btree (user_id);


--
-- Name: idx_gifts_delivery; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_gifts_delivery ON public.gifts USING btree (delivery_status);


--
-- Name: idx_gifts_recipient; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_gifts_recipient ON public.gifts USING btree (recipient_email);


--
-- Name: idx_gifts_token; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_gifts_token ON public.gifts USING btree (redemption_token);


--
-- Name: idx_lesson_reviews_enrollment; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_lesson_reviews_enrollment ON public.lesson_reviews USING btree (enrollment_id);


--
-- Name: idx_lesson_reviews_lesson; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_lesson_reviews_lesson ON public.lesson_reviews USING btree (lesson_id);


--
-- Name: idx_lessons_course; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_lessons_course ON public.lessons USING btree (course_id);


--
-- Name: idx_material_duration; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_material_duration ON public.learning_materials USING btree ((((material_metadata ->> 'duration'::text))::integer));


--
-- Name: idx_materials_lesson; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_materials_lesson ON public.learning_materials USING btree (lesson_id);


--
-- Name: idx_metadata_gin; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_metadata_gin ON public.learning_materials USING gin (material_metadata);


--
-- Name: idx_order_info_user; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_order_info_user ON public.order_info USING btree (user_id);


--
-- Name: idx_order_items_order; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_order_items_order ON public.order_items USING btree (order_id);


--
-- Name: idx_order_paid; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_order_paid ON public.order_info USING btree (user_id) WHERE ((order_status)::text = 'paid'::text);


--
-- Name: ix_lockouts_account_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_lockouts_account_id ON public.lockouts USING btree (account_id);


--
-- Name: audit_logs audit_logs_actor_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.audit_logs
    ADD CONSTRAINT audit_logs_actor_id_fkey FOREIGN KEY (actor_id) REFERENCES public.accounts(account_id) ON DELETE SET NULL;


--
-- Name: cart_items cart_items_course_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cart_items
    ADD CONSTRAINT cart_items_course_id_fkey FOREIGN KEY (course_id) REFERENCES public.courses(course_id) ON DELETE CASCADE;


--
-- Name: cart_items cart_items_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cart_items
    ADD CONSTRAINT cart_items_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(user_id) ON DELETE CASCADE;


--
-- Name: chat_participants chat_participants_account_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.chat_participants
    ADD CONSTRAINT chat_participants_account_id_fkey FOREIGN KEY (account_id) REFERENCES public.accounts(account_id) ON DELETE CASCADE;


--
-- Name: chat_participants chat_participants_chat_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.chat_participants
    ADD CONSTRAINT chat_participants_chat_id_fkey FOREIGN KEY (chat_id) REFERENCES public.chats(chat_id) ON DELETE CASCADE;


--
-- Name: coupons coupons_manager_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.coupons
    ADD CONSTRAINT coupons_manager_id_fkey FOREIGN KEY (manager_id) REFERENCES public.managers(manager_id) ON DELETE SET NULL;


--
-- Name: course_ai_usage_logs course_ai_usage_logs_integration_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.course_ai_usage_logs
    ADD CONSTRAINT course_ai_usage_logs_integration_id_fkey FOREIGN KEY (integration_id) REFERENCES public.courses_ai_integrations(id) ON DELETE SET NULL;


--
-- Name: course_exts course_exts_course_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.course_exts
    ADD CONSTRAINT course_exts_course_id_fkey FOREIGN KEY (course_id) REFERENCES public.courses(course_id) ON DELETE CASCADE;


--
-- Name: course_reports course_reports_course_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.course_reports
    ADD CONSTRAINT course_reports_course_id_fkey FOREIGN KEY (course_id) REFERENCES public.courses(course_id) ON DELETE SET NULL;


--
-- Name: course_reports course_reports_reporter_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.course_reports
    ADD CONSTRAINT course_reports_reporter_id_fkey FOREIGN KEY (reporter_id) REFERENCES public.accounts(account_id) ON DELETE SET NULL;


--
-- Name: course_reports course_reports_resolver_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.course_reports
    ADD CONSTRAINT course_reports_resolver_id_fkey FOREIGN KEY (resolver_id) REFERENCES public.accounts(account_id) ON DELETE SET NULL;


--
-- Name: course_review_moderation_logs course_review_moderation_logs_course_review_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.course_review_moderation_logs
    ADD CONSTRAINT course_review_moderation_logs_course_review_id_fkey FOREIGN KEY (course_review_id) REFERENCES public.course_reviews(course_review_id) ON DELETE SET NULL;


--
-- Name: course_review_moderation_logs course_review_moderation_logs_model_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.course_review_moderation_logs
    ADD CONSTRAINT course_review_moderation_logs_model_id_fkey FOREIGN KEY (model_id) REFERENCES public.ai_models(model_id) ON DELETE SET NULL;


--
-- Name: course_review_reports course_review_reports_course_review_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.course_review_reports
    ADD CONSTRAINT course_review_reports_course_review_id_fkey FOREIGN KEY (course_review_id) REFERENCES public.course_reviews(course_review_id) ON DELETE SET NULL;


--
-- Name: course_review_reports course_review_reports_reporter_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.course_review_reports
    ADD CONSTRAINT course_review_reports_reporter_id_fkey FOREIGN KEY (reporter_id) REFERENCES public.accounts(account_id) ON DELETE SET NULL;


--
-- Name: course_review_reports course_review_reports_resolver_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.course_review_reports
    ADD CONSTRAINT course_review_reports_resolver_id_fkey FOREIGN KEY (resolver_id) REFERENCES public.accounts(account_id) ON DELETE SET NULL;


--
-- Name: course_reviews course_reviews_enrollment_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.course_reviews
    ADD CONSTRAINT course_reviews_enrollment_id_fkey FOREIGN KEY (enrollment_id) REFERENCES public.enrollments(enrollment_id) ON DELETE CASCADE;


--
-- Name: courses_ai_integrations courses_ai_integrations_course_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.courses_ai_integrations
    ADD CONSTRAINT courses_ai_integrations_course_id_fkey FOREIGN KEY (course_id) REFERENCES public.courses(course_id) ON DELETE SET NULL;


--
-- Name: courses_ai_integrations courses_ai_integrations_model_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.courses_ai_integrations
    ADD CONSTRAINT courses_ai_integrations_model_id_fkey FOREIGN KEY (model_id) REFERENCES public.ai_models(model_id) ON DELETE SET NULL;


--
-- Name: courses courses_category_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.courses
    ADD CONSTRAINT courses_category_id_fkey FOREIGN KEY (category_id) REFERENCES public.categories(category_id) ON DELETE SET NULL;


--
-- Name: courses courses_coupon_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.courses
    ADD CONSTRAINT courses_coupon_id_fkey FOREIGN KEY (coupon_id) REFERENCES public.coupons(coupon_id) ON DELETE SET NULL;


--
-- Name: courses courses_instructor_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.courses
    ADD CONSTRAINT courses_instructor_id_fkey FOREIGN KEY (instructor_id) REFERENCES public.instructors(instructor_id) ON DELETE SET NULL;


--
-- Name: enrollments enrollments_course_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.enrollments
    ADD CONSTRAINT enrollments_course_id_fkey FOREIGN KEY (course_id) REFERENCES public.courses(course_id) ON DELETE SET NULL;


--
-- Name: enrollments enrollments_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.enrollments
    ADD CONSTRAINT enrollments_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(user_id) ON DELETE SET NULL;


--
-- Name: gifts gifts_claimed_by_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.gifts
    ADD CONSTRAINT gifts_claimed_by_user_id_fkey FOREIGN KEY (claimed_by_user_id) REFERENCES public.users(user_id) ON DELETE SET NULL;


--
-- Name: gifts gifts_order_item_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.gifts
    ADD CONSTRAINT gifts_order_item_id_fkey FOREIGN KEY (order_item_id) REFERENCES public.order_items(id) ON DELETE CASCADE;


--
-- Name: gifts gifts_sender_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.gifts
    ADD CONSTRAINT gifts_sender_id_fkey FOREIGN KEY (sender_id) REFERENCES public.accounts(account_id) ON DELETE SET NULL;


--
-- Name: instructor_payouts instructor_payouts_instructor_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.instructor_payouts
    ADD CONSTRAINT instructor_payouts_instructor_id_fkey FOREIGN KEY (instructor_id) REFERENCES public.instructors(instructor_id) ON DELETE SET NULL;


--
-- Name: instructor_payouts instructor_payouts_transaction_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.instructor_payouts
    ADD CONSTRAINT instructor_payouts_transaction_id_fkey FOREIGN KEY (transaction_id) REFERENCES public.transactions(transaction_id) ON DELETE SET NULL;


--
-- Name: instructors instructors_instructor_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.instructors
    ADD CONSTRAINT instructors_instructor_id_fkey FOREIGN KEY (instructor_id) REFERENCES public.users(user_id) ON DELETE CASCADE;


--
-- Name: learning_materials learning_materials_lesson_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.learning_materials
    ADD CONSTRAINT learning_materials_lesson_id_fkey FOREIGN KEY (lesson_id) REFERENCES public.lessons(lesson_id) ON DELETE CASCADE;


--
-- Name: lesson_review_moderation_logs lesson_review_moderation_logs_lesson_review_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.lesson_review_moderation_logs
    ADD CONSTRAINT lesson_review_moderation_logs_lesson_review_id_fkey FOREIGN KEY (lesson_review_id) REFERENCES public.lesson_reviews(lesson_review_id) ON DELETE SET NULL;


--
-- Name: lesson_review_moderation_logs lesson_review_moderation_logs_model_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.lesson_review_moderation_logs
    ADD CONSTRAINT lesson_review_moderation_logs_model_id_fkey FOREIGN KEY (model_id) REFERENCES public.ai_models(model_id) ON DELETE SET NULL;


--
-- Name: lesson_review_reports lesson_review_reports_lesson_review_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.lesson_review_reports
    ADD CONSTRAINT lesson_review_reports_lesson_review_id_fkey FOREIGN KEY (lesson_review_id) REFERENCES public.lesson_reviews(lesson_review_id) ON DELETE SET NULL;


--
-- Name: lesson_review_reports lesson_review_reports_reporter_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.lesson_review_reports
    ADD CONSTRAINT lesson_review_reports_reporter_id_fkey FOREIGN KEY (reporter_id) REFERENCES public.accounts(account_id) ON DELETE SET NULL;


--
-- Name: lesson_review_reports lesson_review_reports_resolver_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.lesson_review_reports
    ADD CONSTRAINT lesson_review_reports_resolver_id_fkey FOREIGN KEY (resolver_id) REFERENCES public.accounts(account_id) ON DELETE SET NULL;


--
-- Name: lesson_reviews lesson_reviews_enrollment_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.lesson_reviews
    ADD CONSTRAINT lesson_reviews_enrollment_id_fkey FOREIGN KEY (enrollment_id) REFERENCES public.enrollments(enrollment_id) ON DELETE CASCADE;


--
-- Name: lesson_reviews lesson_reviews_lesson_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.lesson_reviews
    ADD CONSTRAINT lesson_reviews_lesson_id_fkey FOREIGN KEY (lesson_id) REFERENCES public.lessons(lesson_id) ON DELETE SET NULL;


--
-- Name: lessons lessons_course_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.lessons
    ADD CONSTRAINT lessons_course_id_fkey FOREIGN KEY (course_id) REFERENCES public.courses(course_id) ON DELETE CASCADE;


--
-- Name: lockouts lockouts_account_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.lockouts
    ADD CONSTRAINT lockouts_account_id_fkey FOREIGN KEY (account_id) REFERENCES public.accounts(account_id) ON DELETE SET NULL;


--
-- Name: managers managers_manager_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.managers
    ADD CONSTRAINT managers_manager_id_fkey FOREIGN KEY (manager_id) REFERENCES public.accounts(account_id) ON DELETE CASCADE;


--
-- Name: material_completions material_completions_enrollment_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.material_completions
    ADD CONSTRAINT material_completions_enrollment_id_fkey FOREIGN KEY (enrollment_id) REFERENCES public.enrollments(enrollment_id) ON DELETE CASCADE;


--
-- Name: material_completions material_completions_material_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.material_completions
    ADD CONSTRAINT material_completions_material_id_fkey FOREIGN KEY (material_id) REFERENCES public.learning_materials(material_id) ON DELETE CASCADE;


--
-- Name: media_embeddings media_embeddings_material_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.media_embeddings
    ADD CONSTRAINT media_embeddings_material_id_fkey FOREIGN KEY (material_id) REFERENCES public.learning_materials(material_id) ON DELETE CASCADE;


--
-- Name: message_attachments message_attachments_message_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.message_attachments
    ADD CONSTRAINT message_attachments_message_id_fkey FOREIGN KEY (message_id) REFERENCES public.messages(message_id) ON DELETE CASCADE;


--
-- Name: message_moderation_logs message_moderation_logs_message_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.message_moderation_logs
    ADD CONSTRAINT message_moderation_logs_message_id_fkey FOREIGN KEY (message_id) REFERENCES public.messages(message_id) ON DELETE SET NULL;


--
-- Name: message_moderation_logs message_moderation_logs_model_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.message_moderation_logs
    ADD CONSTRAINT message_moderation_logs_model_id_fkey FOREIGN KEY (model_id) REFERENCES public.ai_models(model_id) ON DELETE SET NULL;


--
-- Name: messages messages_chat_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.messages
    ADD CONSTRAINT messages_chat_id_fkey FOREIGN KEY (chat_id) REFERENCES public.chats(chat_id) ON DELETE CASCADE;


--
-- Name: messages messages_sender_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.messages
    ADD CONSTRAINT messages_sender_id_fkey FOREIGN KEY (sender_id) REFERENCES public.accounts(account_id) ON DELETE SET NULL;


--
-- Name: notifications notifications_receiver_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.notifications
    ADD CONSTRAINT notifications_receiver_id_fkey FOREIGN KEY (receiver_id) REFERENCES public.accounts(account_id) ON DELETE CASCADE;


--
-- Name: notifications notifications_sender_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.notifications
    ADD CONSTRAINT notifications_sender_id_fkey FOREIGN KEY (sender_id) REFERENCES public.accounts(account_id) ON DELETE CASCADE;


--
-- Name: order_info order_info_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.order_info
    ADD CONSTRAINT order_info_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(user_id) ON DELETE SET NULL;


--
-- Name: order_items order_items_course_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.order_items
    ADD CONSTRAINT order_items_course_id_fkey FOREIGN KEY (course_id) REFERENCES public.courses(course_id) ON DELETE SET NULL;


--
-- Name: order_items order_items_order_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.order_items
    ADD CONSTRAINT order_items_order_id_fkey FOREIGN KEY (order_id) REFERENCES public.order_info(order_id) ON DELETE CASCADE;


--
-- Name: platform_withdrawals platform_withdrawals_manager_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.platform_withdrawals
    ADD CONSTRAINT platform_withdrawals_manager_id_fkey FOREIGN KEY (manager_id) REFERENCES public.managers(manager_id) ON DELETE SET NULL;


--
-- Name: system_configs system_configs_manager_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.system_configs
    ADD CONSTRAINT system_configs_manager_id_fkey FOREIGN KEY (manager_id) REFERENCES public.managers(manager_id) ON DELETE SET NULL;


--
-- Name: text_embeddings text_embeddings_material_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.text_embeddings
    ADD CONSTRAINT text_embeddings_material_id_fkey FOREIGN KEY (material_id) REFERENCES public.learning_materials(material_id) ON DELETE CASCADE;


--
-- Name: transaction_exts transaction_exts_transaction_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.transaction_exts
    ADD CONSTRAINT transaction_exts_transaction_id_fkey FOREIGN KEY (transaction_id) REFERENCES public.transactions(transaction_id) ON DELETE CASCADE;


--
-- Name: transactions transactions_account_from_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.transactions
    ADD CONSTRAINT transactions_account_from_fkey FOREIGN KEY (account_from) REFERENCES public.accounts(account_id) ON DELETE SET NULL;


--
-- Name: transactions transactions_account_to_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.transactions
    ADD CONSTRAINT transactions_account_to_fkey FOREIGN KEY (account_to) REFERENCES public.accounts(account_id) ON DELETE SET NULL;


--
-- Name: transactions transactions_order_item_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.transactions
    ADD CONSTRAINT transactions_order_item_id_fkey FOREIGN KEY (order_item_id) REFERENCES public.order_items(id) ON DELETE SET NULL;


--
-- Name: user_avatar_frames user_avatar_frames_frame_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_avatar_frames
    ADD CONSTRAINT user_avatar_frames_frame_id_fkey FOREIGN KEY (frame_id) REFERENCES public.avatar_frames(id) ON DELETE CASCADE;


--
-- Name: user_avatar_frames user_avatar_frames_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_avatar_frames
    ADD CONSTRAINT user_avatar_frames_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.accounts(account_id) ON DELETE CASCADE;


--
-- Name: users users_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.accounts(account_id) ON DELETE CASCADE;


--
-- Name: wishlist_items wishlist_items_course_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.wishlist_items
    ADD CONSTRAINT wishlist_items_course_id_fkey FOREIGN KEY (course_id) REFERENCES public.courses(course_id) ON DELETE CASCADE;


--
-- Name: wishlist_items wishlist_items_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.wishlist_items
    ADD CONSTRAINT wishlist_items_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(user_id) ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

\unrestrict 0W7krHvs2wdC5fLvfa0PUQVUWvZFpE0P9o5o5ZJAcneiMwOoloGYQ4DRezm0zSL

