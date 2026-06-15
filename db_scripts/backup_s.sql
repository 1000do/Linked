--
-- PostgreSQL database dump
--

\restrict UMBKoqozV7a1tK5rG6vBQg8bpv8sJBGBxLF3ste0dtvPcFRZgvqAf4Bx6B90AMW

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
    CONSTRAINT instructor_payouts_payout_status_check CHECK (((payout_status)::text = ANY (ARRAY[('pending'::character varying)::text, ('transferred'::character varying)::text, ('in_transit'::character varying)::text, ('paid'::character varying)::text, ('failed'::character varying)::text, ('refunded'::character varying)::text])))
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
    CONSTRAINT platform_withdrawals_status_check CHECK (((status)::text = ANY (ARRAY[('pending'::character varying)::text, ('in_transit'::character varying)::text, ('paid'::character varying)::text, ('failed'::character varying)::text, ('canceled'::character varying)::text])))
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

\unrestrict UMBKoqozV7a1tK5rG6vBQg8bpv8sJBGBxLF3ste0dtvPcFRZgvqAf4Bx6B90AMW

