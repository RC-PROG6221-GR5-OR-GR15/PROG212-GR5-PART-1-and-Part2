Create Database EventManagementDB;

USE EventManagementDB;
--User Table
CREATE TABLE `USER` (
    user_id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(100) NOT NULL UNIQUE,
    email VARCHAR(150) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    date_of_birth DATE,
    phone VARCHAR(20),
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    last_login DATETIME NULL,
    is_active BOOLEAN DEFAULT TRUE
);
--Role Table
CREATE TABLE `ROLE` (
    role_id INT AUTO_INCREMENT PRIMARY KEY,
    role_name VARCHAR(100) NOT NULL UNIQUE,
    description VARCHAR(255),
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);
--User Role

CREATE TABLE USERROLE (
    userrole_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    role_id INT NOT NULL,
    assigned_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    assigned_by INT,

    CONSTRAINT fk_userrole_user
        FOREIGN KEY (user_id)
        REFERENCES `USER`(user_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_userrole_role
        FOREIGN KEY (role_id)
        REFERENCES `ROLE`(role_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_userrole_assigned_by
        FOREIGN KEY (assigned_by)
        REFERENCES `USER`(user_id)
        ON DELETE SET NULL,

    UNIQUE (user_id, role_id)
);

--Event Table
CREATE TABLE EVENT (
    event_id INT AUTO_INCREMENT PRIMARY KEY,
    event_name VARCHAR(150) NOT NULL,
    event_date DATE NOT NULL,
    event_time TIME NOT NULL,
    venue VARCHAR(150),
    location VARCHAR(150),
    distance_km DECIMAL(10,2),
    event_type VARCHAR(100),
    entry_fee DECIMAL(10,2) DEFAULT 0.00,
    status VARCHAR(50) DEFAULT 'Upcoming',
    created_by INT NOT NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_event_created_by
        FOREIGN KEY (created_by)
        REFERENCES `USER`(user_id)
        ON DELETE RESTRICT
);
-- Category Table
CREATE TABLE CATEGORY (
    category_id INT AUTO_INCREMENT PRIMARY KEY,
    event_id INT NOT NULL,
    category_name VARCHAR(100) NOT NULL,
    min_age INT,
    max_age INT,
    gender_restriction VARCHAR(50),
    max_participants INT,
    entry_fee DECIMAL(10,2) DEFAULT 0.00,
    created_by INT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_category_event
        FOREIGN KEY (event_id)
        REFERENCES EVENT(event_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_category_created_by
        FOREIGN KEY (created_by)
        REFERENCES `USER`(user_id)
        ON DELETE SET NULL,

    CHECK (min_age IS NULL OR max_age IS NULL OR min_age <= max_age)
);
--Enrolment Table
CREATE TABLE ENROLMENT (
    enrolment_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    event_id INT NOT NULL,
    category_id INT NOT NULL,
    enrolment_date DATETIME DEFAULT CURRENT_TIMESTAMP,
    enrolment_status VARCHAR(50) DEFAULT 'Pending',
    payment_status VARCHAR(50) DEFAULT 'Unpaid',
    amount_paid DECIMAL(10,2) DEFAULT 0.00,
    payment_date DATETIME NULL,
    created_by INT,

    CONSTRAINT fk_enrolment_user
        FOREIGN KEY (user_id)
        REFERENCES `USER`(user_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_enrolment_event
        FOREIGN KEY (event_id)
        REFERENCES EVENT(event_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_enrolment_category
        FOREIGN KEY (category_id)
        REFERENCES CATEGORY(category_id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_enrolment_created_by
        FOREIGN KEY (created_by)
        REFERENCES `USER`(user_id)
        ON DELETE SET NULL,

    UNIQUE (user_id, event_id)
);
--Participant event Table
CREATE TABLE PARTICIPANT_EVENT (
    participant_event_id INT AUTO_INCREMENT PRIMARY KEY,
    event_id INT NOT NULL,
    user_id INT NOT NULL,
    category_id INT NOT NULL,
    bib_number INT,
    finishing_position INT,
    finish_time_seconds DECIMAL(12,2),
    entry_status VARCHAR(50) DEFAULT 'Confirmed',
    entered_at DATETIME DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_participant_event_event
        FOREIGN KEY (event_id)
        REFERENCES EVENT(event_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_participant_event_user
        FOREIGN KEY (user_id)
        REFERENCES `USER`(user_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_participant_event_category
        FOREIGN KEY (category_id)
        REFERENCES CATEGORY(category_id)
        ON DELETE RESTRICT,

    UNIQUE (event_id, user_id),
    UNIQUE (event_id, bib_number)
);
-- Results Table 
CREATE TABLE RESULT (
    result_id INT AUTO_INCREMENT PRIMARY KEY,
    participant_event_id INT NOT NULL,
    finishing_position INT,
    finish_time_seconds DECIMAL(12,2),
    pace_per_km DECIMAL(10,2),
    overall_rank INT,
    category_rank INT,
    prize_money DECIMAL(10,2) DEFAULT 0.00,

    CONSTRAINT fk_result_participant
        FOREIGN KEY (participant_event_id)
        REFERENCES PARTICIPANT_EVENT(participant_event_id)
        ON DELETE CASCADE,

    UNIQUE (participant_event_id)
);

-- SEED DATA
-- Seed Roles
INSERT INTO `ROLE` (role_name, description) VALUES 
('Organiser', 'Manages events, categories, and results'),
('Participant', 'Enrols in events and views personal results'),
('Admin', 'Full administrative access to events, users, roles and results');

-- Seed Users
-- All seeded accounts below use the password: Passw0rd!
-- password_hash values are ASP.NET Core Identity PBKDF2 (IdentityV3, HMAC-SHA256, 100 000 iterations)
-- hashes, so they verify against PasswordHasher<T> in the API and can be used to sign in immediately.
INSERT INTO `USER` (username, email, password_hash, first_name, last_name, date_of_birth, phone) VALUES 
('jdoe_admin', 'johndoe@example.com', 'AQAAAAEAAYagAAAAEB9wjqUf5IvHKsH+P3dr0p7R3/zVFxM3QJN3KA3RJJBPv/7s/Gxr2rRdneWY/n/5wg==', 'John', 'Doe', '1985-05-15', '0123456789'),
('asmith_org', 'alicesmith@example.com', 'AQAAAAEAAYagAAAAEGOAiAGm/04aPMqGtS1vFVzL/obwbUEB7iMdDLr6oUiIrvyL8W536bkIVn1o1UAHUw==', 'Alice', 'Smith', '1990-08-22', '0987654321'),
('bwilson_part', 'bobwilson@example.com', 'AQAAAAEAAYagAAAAENOkxb/KTkYwo/cWIjTmUq2ThknL70wZOA8GPull/q/1Tf9ShuYgN76Vczihz8F/qg==', 'Bob', 'Wilson', '1995-12-01', '0555123456'),
('cgreen_part', 'charliegreen@example.com', 'AQAAAAEAAYagAAAAEEPLwm3WMvK/Cpp08YaB6ysesnRMjwcnHb5+witBPuv8JrwlMvA2j+vK0QsVhj+nTQ==', 'Charlie', 'Green', '1998-03-10', '0555987654');

-- Seed User Roles
INSERT INTO USERROLE (user_id, role_id, assigned_by) VALUES 
(1, 1, 1), -- John as Organiser
(2, 1, 1), -- Alice as Organiser
(3, 2, 1), -- Bob as Participant
(4, 2, 1); -- Charlie as Participant

-- Seed Events
INSERT INTO EVENT (event_name, event_date, event_time, venue, location, distance_km, event_type, entry_fee, status, created_by) VALUES 
('Johannesburg City Marathon', '2026-10-15', '06:00:00', 'FNB Stadium', 'Johannesburg', 42.20, 'Run', 250.00, 'Upcoming', 1),
('Cape Town Cycle Tour', '2026-11-20', '07:30:00', 'Grand Parade', 'Cape Town', 109.00, 'Cycle', 500.00, 'Upcoming', 2),
('Soweto Community Walk', '2026-12-05', '08:00:00', 'Morris Isaacson High', 'Soweto', 10.00, 'Walk', 50.00, 'Upcoming', 1);

-- Seed Categories
INSERT INTO CATEGORY (event_id, category_name, min_age, max_age, gender_restriction, max_participants, entry_fee, created_by) VALUES 
(1, 'Open Male', 18, 99, 'Male', 5000, 250.00, 1),
(1, 'Open Female', 18, 99, 'Female', 5000, 250.00, 1),
(2, 'Elite Cyclist', 18, 60, 'None', 200, 500.00, 2),
(3, 'Junior Walk', 5, 17, 'None', 1000, 25.00, 1);

-- Seed Enrolments
INSERT INTO ENROLMENT (user_id, event_id, category_id, enrolment_status, payment_status, amount_paid, created_by) VALUES 
(3, 1, 1, 'Confirmed', 'Paid', 250.00, 3),
(4, 1, 2, 'Pending', 'Unpaid', 0.00, 4);

-- Seed Participant Events
INSERT INTO PARTICIPANT_EVENT (event_id, user_id, category_id, bib_number, entry_status) VALUES 
(1, 3, 1, 1001, 'Confirmed');

-- Seed Results
INSERT INTO RESULT (participant_event_id, finishing_position, finish_time_seconds, pace_per_km, overall_rank, category_rank, prize_money) VALUES 
(1, 47, 10800, 255.92, 47, 12, 0.00);

